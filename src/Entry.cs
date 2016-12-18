using System;

namespace AuthSharp
{
    public class Entry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string Secret { get; set; }
        public EntryType Type { get; set; }
    }
}