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

        this.nodes = [];
        this.links = [];

        this.svg = d3.select('.resource-graph');
        this.baseGroup = this.svg.append("g");

        // Enable zoom + pan
        // https://www.d3indepth.com/zoom-and-pan/
        let zoom = d3.zoom().on('zoom', (event) => {
            this.baseGroup.attr('transform', event.transform);
        });
        this.svg.call(zoom);

        // simulation setup with all forces
        this.linkForce = d3
            .forceLink()
            .id(function (link) { return link.id })
            .strength(function (link) { return 1 })
            .distance(150);

        this.simulation = d3
            .forceSimulation()
            .force('link', this.linkForce)
            .force('charge', d3.forceManyBody().strength(-800))
            .force("collide", d3.forceCollide(110).iterations(10))
            .force("x", d3.forceX().strength(0.1))
            .force("y", d3.forceY().strength(0.2));

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
            .attr("refX", 66)
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
            .attr("refX", 48)
            .attr("refY", 0)
            .attr("markerWidth", 15)
            .attr("markerHeight", 15)
            .attr("orient", "auto")
            .attr("markerUnits", "userSpaceOnUse")
            .attr("class", d => `arrow-${d}`)
            .append("path")
            .attr("d", 'M0,-5L10,0L0,5');

        this.linkElementsG = this.baseGroup.append("g").attr("class", "links");
        this.nodeElementsG = this.baseGroup.append("g").attr("class", "nodes");
        this.textElementsG = this.baseGroup.append("g").attr("class", "texts");
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
        //this.simulation.alpha(0.01).restart();
    }

    updateResources(resources) {
        //resources.sort((a, b) => b.referencedNames.length - a.referencedNames.length);

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
                    label: resource.displayName,
                    endpointUrl: resource.endpointUrl,
                    endpointText: resource.endpointText,
                    resourceIcon: {
                        path: resource.resourceIcon.path,
                        color: resource.resourceIcon.color,
                        tooltip: resource.resourceIcon.tooltip
                    },
                    stateIcon: {
                        path: resource.stateIcon.path,
                        color: resource.stateIcon.color,
                        tooltip: resource.stateIcon.tooltip
                    }
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
            .selectAll(".resource-group, .resource-group-selected, .resource-group-hover, .resource-group-highlight")
            .data(this.nodes, n => n.id);

        // Remove excess nodes:
        this.nodeElements
            .exit()
            .transition()
            .attr("opacity", 0)
            .remove();

        // Resource node
        var newNodes = this.nodeElements
            .enter().append("g")
            .attr("class", "resource-group")
            .attr("opacity", 0)
            .attr("resource-name", n => n.id)
            .call(this.dragDrop)
            .on('click', this.selectNode)
            .on('mouseover', this.hoverNode)
            .on('mouseout', this.unHoverNode);
        newNodes
            .append("circle")
            .attr("r", 56)
            .attr("class", "resource-node")
            .attr("stroke", "white")
            .attr("stroke-width", "4");
        newNodes
            .append("circle")
            .attr("r", 53)
            .attr("class", "resource-node-border");
        newNodes
            .append("g")
            .attr("transform", "scale(2.1) translate(-12,-17)")
            .append("path")
            .attr("fill", n => n.resourceIcon.color)
            .attr("d", n => n.resourceIcon.path)
            .append("title")
            .text(n => n.resourceIcon.tooltip);

        newNodes
            .append("text")
            .text(function (node) {
                return node.endpointText || 'No endpoints';
            })
            .attr("class", "resource-endpoint")
            .attr("font-size", 11)
            .attr("text-anchor", "middle")
            .attr("dy", 28);

        // Resource status
        var statusGroup = newNodes
            .append("g")
            .attr("transform", "scale(1.6) translate(14,-34)");
        statusGroup
            .append("circle")
            .attr("r", 8)
            .attr("cy", 8)
            .attr("cx", 8)
            .attr("class", "resource-status-circle")
            .append("title")
            .text(n => n.stateIcon.tooltip);
        statusGroup
            .append("path")
            .attr("d", n => n.stateIcon.path)
            .attr("fill", n => n.stateIcon.color)
            .append("title")
            .text(n => n.stateIcon.tooltip);

        newNodes.transition()
            .attr("opacity", 1);

        this.nodeElements = newNodes.merge(this.nodeElements);

        // Update text
        this.textElements = this.textElementsG
            .selectAll("g")
            .data(this.nodes, n => n.id);

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
            .attr("dy", 71)
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
            .data(this.links, function (d) { return d.source.id + "-" + d.target.id; });

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
        //this.nodeElements
        //    .attr('cx', function (node) { return node.x })
        //    .attr('cy', function (node) { return node.y });

        this.nodeElements.attr("transform", function (d) { return "translate(" + d.x + "," + d.y + ")"; });
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
                neighbors.push(link.source.id);
            } else if (link.source.id === node.id) {
                neighbors.push(link.target.id);
            }
            return neighbors;
        },
            [node.id]);
    }

    isNeighborLink(node, link) {
        return link.target.id === node.id || link.source.id === node.id
    }

    getLinkColor(nodes, link) {
        if (nodes.find(n => this.isNeighborLink(n, link))) {
            return 'resource-link-highlight';
        }
        return 'resource-link';
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

    nodeEquals(resource1, resource2) {
        if (!resource1 || !resource2) {
            return false;
        }
        return resource1.id === resource2.id;
    }

    updateNodeHighlights = (mouseoverNode) => {
        var mouseoverNeighbors = mouseoverNode ? this.getNeighbors(mouseoverNode) : [];
        var selectNeighbors = this.selectedNode ? this.getNeighbors(this.selectedNode) : [];
        var neighbors = [...mouseoverNeighbors, ...selectNeighbors];

        // we modify the styles to highlight selected nodes
        this.nodeElements.attr('class', (node) => {
            if (this.nodeEquals(node, mouseoverNode)) {
                return 'resource-group-hover';
            }
            if (this.nodeEquals(node, this.selectedNode)) {
                return 'resource-group-selected';
            }

            if (neighbors.indexOf(node.id) > -1) {
                return 'resource-group-highlight';
            }
            return 'resource-group';
        });
        this.textElements.attr('class', (node) => {
            if (this.nodeEquals(node, this.selectedNode)) {
                return 'resource-name-selected';
            }

            return 'resource-name';
        });
        this.linkElements.attr('class', (link) => {
            var nodes = [];
            if (mouseoverNode) {
                nodes.push(mouseoverNode);
            }
            if (this.selectedNode) {
                nodes.push(this.selectedNode);
            }
            return this.getLinkColor(nodes, link);
        });
    };
};
