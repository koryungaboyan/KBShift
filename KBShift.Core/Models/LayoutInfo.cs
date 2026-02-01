namespace KBShift.Core.Models
{
    public class LayoutInfo
    {
        public string LayoutId { get; set; }
        public string CultureName { get; set; }
        public string DisplayName { get; set; }

        public LayoutInfo(string layoutId, string cultureName, string displayName)
        {
            LayoutId = layoutId;
            CultureName = cultureName;
            DisplayName = displayName;
        }
    }
}