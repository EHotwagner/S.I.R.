#!/usr/bin/env python3
"""Repository-owned structural validator for S.I.R. skill packages."""

import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
sys.path.insert(0, str(Path(__file__).resolve().parent / "vendor"))
import yaml  # noqa: E402 - repository-owned PyYAML is selected before ambient packages

ALLOWED_PROPERTIES = {"name", "description", "license", "allowed-tools", "metadata"}


def validate(skill_path: Path) -> tuple[bool, str]:
    skill_file = skill_path / "SKILL.md"
    if not skill_file.is_file():
        return False, "SKILL.md not found"

    content = skill_file.read_text(encoding="utf-8")
    match = re.match(r"^---\n(.*?)\n---", content, re.DOTALL)
    if not match:
        return False, "Invalid or missing YAML frontmatter"

    try:
        frontmatter = yaml.safe_load(match.group(1))
    except yaml.YAMLError as error:
        return False, f"Invalid YAML in frontmatter: {error}"
    if not isinstance(frontmatter, dict):
        return False, "Frontmatter must be a YAML dictionary"

    unexpected = set(frontmatter) - ALLOWED_PROPERTIES
    if unexpected:
        return False, "Unexpected frontmatter key(s): " + ", ".join(sorted(unexpected))

    name = frontmatter.get("name")
    description = frontmatter.get("description")
    if not isinstance(name, str) or not name.strip():
        return False, "Missing or invalid 'name'"
    if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", name) or len(name) > 64:
        return False, "Skill name must be hyphen-case and at most 64 characters"
    if not isinstance(description, str) or not description.strip():
        return False, "Missing or invalid 'description'"
    if len(description) > 1024 or "<" in description or ">" in description:
        return False, "Skill description is invalid or exceeds 1024 characters"

    return True, "Skill is valid!"


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: validate-skill-package.py <skill-directory>", file=sys.stderr)
        raise SystemExit(2)
    valid, message = validate(Path(sys.argv[1]))
    print(message)
    raise SystemExit(0 if valid else 1)
