let resourceGraph = null;

window.initializeResourcesGraph = function (resourcesInterop) {
    resourceGraph = new ResourceGraph(resourcesInterop);
    resourceGraph.resize();

    const observer = new ResizeObserver(function () {
        resourceGraph.resize();
    });

    for (const child of document.getElementsByClassName('resources-summary-layout')) {
        observer.observe(child);
    }
};

window.updateResourcesGraph = function (resources) {
    if (resourceGraph) {
        resourceGraph.updateResources(resources);
    }
};

window.updateResourcesGraphSelected = function (resourceName) {
    if (resourceGraph) {
        resourceGraph.switchTo(resourceName);
    }
}

class ResourceGraph {
    constructor(resourcesInterop) {
        this.resourcesInterop = resourcesInterop;

        this.backNodes = [];
        this.nodes = [];
        this.links = [];

        this.svg = d3.select('.resource-graph');

        // simulation setup with all forces
        this.linkForce = d3
            .forceLink()
            .id(function (link) { return link.id })
            .strength(function (link) { return 1 })
            .distance(100);

        this.simulation = d3
            .forceSimulation()
            .force('link', this.linkForce)
            .force('charge', d3.forceManyBody().strength(-800))
            .force("collide", d3.forceCollide(70).iterations(10))
            .force("x", d3.forceX().strength(0.1))
            .force("y", d3.forceY().strength(0.15));

        this.dragDrop = d3.drag().on('start', (event) => {
            if (!event.active) {
                this.simulation.alphaTarget(0.3).restart();
            }
            event.subject.fx = event.subject.x;
            event.subject.fy = event.subject.y;
        }).on('drag', (event) => {
            event.subject.fx = event.x;
            event.subject.fy = event.y;
        }).on('end', (event) => {
            if (!event.active) {
                this.simulation.alphaTarget(0);
            }
            event.subject.fx = null;
            event.subject.fy = null;
        });

        this.statuses1 = ["normal"];

        this.svg.append("defs").selectAll("marker")
            .data(this.statuses1)
            .join("marker")
            .attr("id", d => `arrow-${d}`)
            .attr("viewBox", "0 -5 10 10")
            .attr("refX", 42)
            .attr("refY", 0)
            .attr("markerWidth", 10)
            .attr("markerHeight", 10)
            .attr("orient", "auto")
            .attr("markerUnits", "userSpaceOnUse")
            .attr("class", d => `arrow-${d}`)
            .append("path")
            .attr("d", 'M0,-5L10,0L0,5');

        this.statuses2 = ["highlight"];

        this.svg.append("defs").selectAll("marker")
            .data(this.statuses2)
            .join("marker")
            .attr("id", d => `arrow-${d}`)
            .attr("viewBox", "0 -5 10 10")
            .attr("refX", 30)
            .attr("refY", 0)
            .attr("markerWidth", 15)
            .attr("markerHeight", 15)
            .attr("orient", "auto")
            .attr("markerUnits", "userSpaceOnUse")
            .attr("class", d => `arrow-${d}`)
            .append("path")
            .attr("d", 'M0,-5L10,0L0,5');

        this.linkElementsG = this.svg.append("g").attr("class", "links");
        this.nodeElementsG = this.svg.append("g").attr("class", "nodes");
        this.textElementsG = this.svg.append("g").attr("class", "texts");
    }

    resize() {
        var container = document.getElementsByClassName("resources-summary-layout")[0];
        var width = container.clientWidth;
        var height = Math.max(container.clientHeight - 50, 0);
        this.svg.attr("viewBox", [-width / 2, -height / 2, width, height]);
    }

    switchTo(resourceName) {
        this.selectedNode = this.nodes.find(node => node.id === resourceName);
        this.updateNodeHighlights(null);

        // For some reason the arrow markers on lines disappear when switching back to
        // Update the simulation
        this.simulation.alpha(0.01).restart();
    }

    updateResources(resources) {
        resources.sort((a, b) => b.referencedNames.length - a.referencedNames.length);

        // If the resources are the same then quickly exit.
        // TODO: Replace JSON.stringify with lower-level comparison.
        if (this.resources && JSON.stringify(resources) === JSON.stringify(this.resources)) {
            return;
        }

        this.resources = resources;

        this.nodes = resources
            .map((resource, index) => {
                return {
                    id: resource.name,
                    group: 1,
                    label: resource.displayName,
                    level: 1,
                    endpointUrl: resource.endpointUrl,
                    endpointText: resource.endpointText,
                    color: resource.color
                };
            });

        this.links = [];
        for (var i = 0; i < resources.length; i++) {
            var resource = resources[i];

            var resourceLinks = resource.referencedNames.map((referencedName, index) => {
                return { target: referencedName, source: resource.name, strength: 0.7 };
            });

            this.links.push(...resourceLinks);
        }

        // Update nodes
        this.nodeElements = this.nodeElementsG
            .selectAll("circle")
            .data(this.nodes);

        // Remove excess nodes:
        this.nodeElements
            .exit()
            .transition()
            .attr("opacity", 0)
            .remove();

        var newNodes = this.nodeElements
            .enter().append("circle")
            .attr("opacity", 0)
            .attr("r", 30)
            .attr("class", "resource-node")
            .attr("fill", n => n.color)
            .attr("stroke", "white")
            .attr("stroke-width", "0.5em")
            .call(this.dragDrop)
            .on('click', this.selectNode)
            .on('mouseover', this.hoverNode)
            .on('mouseout', this.unHoverNode);

        newNodes.transition()
            .attr("opacity", 1);

        this.nodeElements = newNodes.merge(this.nodeElements);

        // Update text
        this.textElements = this.textElementsG
            .selectAll("g")
            .data(this.nodes);

        // Remove excess text:
        this.textElements
            .exit()
            .transition()
            .attr("opacity", 0)
            .remove();

        var newText = this.textElements
            .enter().append("g")
            .attr("opacity", 0)
            .call(this.dragDrop)
            .on('mouseover', this.hoverNode)
            .on('mouseout', this.unHoverNode);

        newText
            .append("text")
            .text(function (node) {
                return node.label;
            })
            .attr("class", "resource-name")
            .attr("font-size", 15)
            .attr("text-anchor", "middle")
            .attr("stroke", "white")
            .attr("stroke-width", "0.5em")
            .attr("paint-order", "stroke")
            .attr("stroke-linejoin", "round")
            .attr("dy", 45)
            .on('click', this.selectNode);
/*
        newText
            .append("text")
            .text(function (node) {
                return node.endpointUrl;
            })
            .attr("font-size", 15)
            .attr("text-anchor", "middle")
            .attr("dy", 50)
            .on('click', this.selectNode);
            */
        newText.transition()
            .attr("opacity", 1);

        this.textElements = newText.merge(this.textElements);

        // Update links
        this.linkElements = this.linkElementsG
            .selectAll("line")
            .data(this.links);

        this.linkElements
            .exit()
            .transition()
            .attr("opacity", 0)
            .remove();

        var newLinks = this.linkElements
            .enter().append("line")
            .attr("opacity", 0)
            .attr("class", "resource-link");

        newLinks.transition()
            .attr("opacity", 1);

        this.linkElements = newLinks.merge(this.linkElements);

        this.simulation
            .nodes(this.nodes)
            .on('tick', this.onTick);

        this.simulation.force("link").links(this.links);
        this.simulation.alpha(1).restart();
   }

    onTick = () => {
        this.nodeElements
            .attr('cx', function (node) { return node.x })
            .attr('cy', function (node) { return node.y });

        this.textElements.attr("transform", function (d) { return "translate(" + d.x + "," + d.y + ")"; });

        this.linkElements
            .attr('x1', function (link) { return link.source.x })
            .attr('y1', function (link) { return link.source.y })
            .attr('x2', function (link) { return link.target.x })
            .attr('y2', function (link) { return link.target.y });
    }

    getNeighbors(node) {
        return this.links.reduce(function (neighbors, link) {
            if (link.target.id === node.id) {
                neighbors.push(link.source.id)
            } else if (link.source.id === node.id) {
                neighbors.push(link.target.id)
            }
            return neighbors
        },
            [node.id]);
    }

    isNeighborLink(node, link) {
        return link.target.id === node.id || link.source.id === node.id
    }

    getNodeColor(node, neighbors) {
        if (Array.isArray(neighbors) && neighbors.indexOf(node.id) > -1) {
            return node.level === 1 ? 'blue' : 'green'
        }

        return node.level === 1 ? node.color : 'gray'
    }

    getLinkColor(nodes, link) {
        if (nodes.find(n => this.isNeighborLink(n, link))) {
            return 'resource-link-highlight';
        }
        return 'resource-link'
    }

    getTextColor(node, neighbors) {
        return Array.isArray(neighbors) && neighbors.indexOf(node.id) > -1 ? 'green' : 'black'
    }

    selectNode = (event) => {
        this.selectedNode = event.target.__data__;

        this.updateNodeHighlights(null);

        this.resourcesInterop.invokeMethodAsync('SelectResource', this.selectedNode.id);
    }

    hoverNode = (event) => {
        var mouseoverNode = event.target.__data__;

        this.updateNodeHighlights(mouseoverNode);
    }

    unHoverNode = (event) => {
        this.updateNodeHighlights(null);
    };

    updateNodeHighlights = (mouseoverNode) => {
        var mouseoverNeighbors = mouseoverNode ? this.getNeighbors(mouseoverNode) : [];
        var selectNeighbors = this.selectedNode ? this.getNeighbors(this.selectedNode) : [];
        var neighbors = [...mouseoverNeighbors, ...selectNeighbors];

        // we modify the styles to highlight selected nodes
        this.nodeElements.attr('fill', (node) => {
            return this.getNodeColor(node, neighbors);
        });
        this.textElements.attr('fill', (node) => {
            return this.getTextColor(node, neighbors);
        });
        this.linkElements.attr('class', (link) => {
            var nodes = [];
            if (mouseoverNode) {
                nodes.push(mouseoverNode);
            }
            if (this.selectedNode) {
                nodes.push(this.selectedNode);
            }
            return this.getLinkColor(nodes, link)
        });
    };
};
