using System.Collections.ObjectModel;

namespace HtmlGenerator
{
    public class HtmlSectionElement : HtmlElement 
    {
        public HtmlSectionElement() : base("section", false) 
        {    
        }

        public new HtmlSectionElement WithChild(HtmlElement child) => (HtmlSectionElement)base.WithChild(child);
        public new HtmlSectionElement WithChildren(Collection<HtmlElement> children) => (HtmlSectionElement)base.WithChildren(children);

        public new HtmlSectionElement WithInnerText(string innerText) => (HtmlSectionElement)base.WithInnerText(innerText);

        public new HtmlSectionElement WithAttribute(HtmlAttribute attribute) => (HtmlSectionElement)base.WithAttribute(attribute);
        public new HtmlSectionElement WithAttributes(Collection<HtmlAttribute> attributes) => (HtmlSectionElement)base.WithAttributes(attributes);

		public HtmlSectionElement WithAccessKey(string value) => WithAttribute(Attribute.AccessKey(value));

		public HtmlSectionElement WithClass(string value) => WithAttribute(Attribute.Class(value));

		public HtmlSectionElement WithContentEditable(string value) => WithAttribute(Attribute.ContentEditable(value));

		public HtmlSectionElement WithContextMenu(string value) => WithAttribute(Attribute.ContextMenu(value));

		public HtmlSectionElement WithDir(string value) => WithAttribute(Attribute.Dir(value));

		public HtmlSectionElement WithHidden(string value) => WithAttribute(Attribute.Hidden(value));

		public HtmlSectionElement WithId(string value) => WithAttribute(Attribute.Id(value));

		public HtmlSectionElement WithLang(string value) => WithAttribute(Attribute.Lang(value));

		public HtmlSectionElement WithSpellCheck(string value) => WithAttribute(Attribute.SpellCheck(value));

		public HtmlSectionElement WithStyle(string value) => WithAttribute(Attribute.Style(value));

		public HtmlSectionElement WithTabIndex(string value) => WithAttribute(Attribute.TabIndex(value));
    }
}
