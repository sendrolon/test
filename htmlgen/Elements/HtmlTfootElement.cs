using System.Collections.ObjectModel;

namespace HtmlGenerator
{
    public class HtmlTfootElement : HtmlElement 
    {
        public HtmlTfootElement() : base("tfoot", false) 
        {    
        }

        public new HtmlTfootElement WithChild(HtmlElement child) => (HtmlTfootElement)base.WithChild(child);
        public new HtmlTfootElement WithChildren(Collection<HtmlElement> children) => (HtmlTfootElement)base.WithChildren(children);

        public new HtmlTfootElement WithInnerText(string innerText) => (HtmlTfootElement)base.WithInnerText(innerText);

        public new HtmlTfootElement WithAttribute(HtmlAttribute attribute) => (HtmlTfootElement)base.WithAttribute(attribute);
        public new HtmlTfootElement WithAttributes(Collection<HtmlAttribute> attributes) => (HtmlTfootElement)base.WithAttributes(attributes);

		public HtmlTfootElement WithAccessKey(string value) => WithAttribute(Attribute.AccessKey(value));

		public HtmlTfootElement WithClass(string value) => WithAttribute(Attribute.Class(value));

		public HtmlTfootElement WithContentEditable(string value) => WithAttribute(Attribute.ContentEditable(value));

		public HtmlTfootElement WithContextMenu(string value) => WithAttribute(Attribute.ContextMenu(value));

		public HtmlTfootElement WithDir(string value) => WithAttribute(Attribute.Dir(value));

		public HtmlTfootElement WithHidden(string value) => WithAttribute(Attribute.Hidden(value));

		public HtmlTfootElement WithId(string value) => WithAttribute(Attribute.Id(value));

		public HtmlTfootElement WithLang(string value) => WithAttribute(Attribute.Lang(value));

		public HtmlTfootElement WithSpellCheck(string value) => WithAttribute(Attribute.SpellCheck(value));

		public HtmlTfootElement WithStyle(string value) => WithAttribute(Attribute.Style(value));

		public HtmlTfootElement WithTabIndex(string value) => WithAttribute(Attribute.TabIndex(value));
    }
}
