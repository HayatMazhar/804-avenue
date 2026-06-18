using Avenue804.Web.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Avenue804.Web.TagHelpers;

/// <summary>
/// Renders editable CMS-managed text. Drop-in replacement for any hardcoded
/// snippet visible to a visitor.
///
/// Usage:
///   &lt;cms key="nav.menu.services"&gt;Services&lt;/cms&gt;
///
/// If a published <see cref="Domain.ContentBlock"/> exists for the slug, its
/// body HTML is rendered. Otherwise the inner content is rendered as the
/// fallback (so the site still works before an admin customises anything).
///
/// By default the tag itself is stripped (no wrapper). Pass <c>tag="div"</c>
/// or <c>tag="span"</c> to keep a wrapper element.
/// </summary>
[HtmlTargetElement("cms", Attributes = KeyAttributeName)]
public class CmsTagHelper : TagHelper
{
    private const string KeyAttributeName = "key";
    private readonly ICmsTextProvider _provider;

    public CmsTagHelper(ICmsTextProvider provider) => _provider = provider;

    [HtmlAttributeName(KeyAttributeName)]
    public string Key { get; set; } = "";

    /// <summary>Optional wrapper tag (e.g. <c>span</c>, <c>div</c>). Default: none.</summary>
    [HtmlAttributeName("tag")]
    public string? Tag { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var body = await _provider.GetAsync(Key);

        if (!string.IsNullOrWhiteSpace(body))
        {
            output.Content.SetHtmlContent(body);
        }
        else
        {
            var inner = await output.GetChildContentAsync();
            output.Content.SetHtmlContent(inner);
        }

        output.TagName = string.IsNullOrWhiteSpace(Tag) ? null : Tag;
        if (output.TagName is null)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}
