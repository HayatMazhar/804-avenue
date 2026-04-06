"""One-off: slice static HTML body into Razor partial. Run from repo root."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def slice_file(src: Path, start_1: int, end_1: int, dest: Path, extra_repl: dict[str, str] | None = None) -> None:
    lines = src.read_text(encoding="utf-8").splitlines(keepends=True)
    body = "".join(lines[start_1 - 1 : end_1])
    body = body.replace('href="../index.html"', 'asp-page="/Index"')
    body = body.replace('href="contact.html"', 'asp-page="/Contact"')
    body = body.replace('href="properties.html"', 'asp-page="/Properties/Index"')
    body = body.replace('href="contracting.html"', 'asp-page="/Contracting"')
    body = body.replace('href="maintenance.html"', 'asp-page="/Maintenance"')
    body = body.replace('href="facility-management.html"', 'asp-page="/FacilityManagement"')
    body = body.replace('href="about.html"', 'asp-page="/About"')
    if extra_repl:
        for k, v in extra_repl.items():
            body = body.replace(k, v)
    dest.write_text(body, encoding="utf-8")


def main() -> None:
    pages = ROOT / "pages"
    shared = ROOT / "src" / "Avenue804.Web" / "Pages" / "Shared"
    slice_file(pages / "contracting.html", 121, 362, shared / "_ContractingBody.cshtml")
    slice_file(pages / "maintenance.html", 132, 321, shared / "_MaintenanceBody.cshtml")
    slice_file(pages / "facility-management.html", 116, 199, shared / "_FacilityManagementBody.cshtml")
    slice_file(pages / "about.html", 135, 342, shared / "_AboutBody.cshtml")
    print("done")


if __name__ == "__main__":
    main()
