namespace ManageMetadataUpdates
{
    internal class MetadataRecord
    {
        public int MetadataRecordId { get; set; }

        public string? Url { get; set; }

        public string? Metadata { get; set; }

        public string? Hash { get; set; }

        public string? Etag { get; set; }

        public DateTime? LastChecked { get; set; }

        public DateTime? LastDownloaded { get; set; }

        public DateTime? LastChanged { get; set; }
    }
}
