from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
PAGES = ROOT / "pages"


def extract_style(path: Path) -> str:
    text = path.read_text(encoding="utf-8")
    m = re.search(r"<style>(.*?)</style>", text, re.DOTALL)
    if not m:
        return ""
    return m.group(1).strip()


def main() -> None:
    contracting = extract_style(PAGES / "contracting.html")
    about = extract_style(PAGES / "about.html")
    maintenance = extract_style(PAGES / "maintenance.html")
    # facility shares svc + why + cta with others; skip duplicate-heavy extract
    combined = f"""/* Service & about pages — merged from static prototype */
{contracting}

/* About page additions */
{about}

/* Maintenance page additions (why-maint, contract-cta, emergency; svc-* may duplicate) */
{maintenance}
"""
    for rel in (
        ROOT / "src" / "Avenue804.Web" / "wwwroot" / "css" / "service-pages.css",
        ROOT / "assets" / "css" / "service-pages.css",
    ):
        rel.parent.mkdir(parents=True, exist_ok=True)
        rel.write_text(combined, encoding="utf-8")
    print("wrote", combined.count("\n"), "lines")


if __name__ == "__main__":
    main()
