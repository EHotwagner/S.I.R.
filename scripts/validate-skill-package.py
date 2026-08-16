#!/usr/bin/env python3
"""Repository-owned structural validator for S.I.R. skill packages."""

import re
import sys
from pathlib import Path

ALLOWED_PROPERTIES = {"name", "description", "license", "allowed-tools", "metadata"}


def validate(skill_path: Path) -> tuple[bool, str]:
    skill_file = skill_path / "SKILL.md"
    if not skill_file.is_file():
        return False, "SKILL.md not found"

    content = skill_file.read_text(encoding="utf-8")
    match = re.match(r"^---\n(.*?)\n---", content, re.DOTALL)
    if not match:
        return False, "Invalid or missing YAML frontmatter"

    frontmatter: dict[str, object] = {}
    lines = match.group(1).splitlines()
    index = 0
    while index < len(lines):
        line = lines[index]
        if not line.strip() or line.lstrip().startswith("#"):
            index += 1
            continue
        field = re.fullmatch(r"([A-Za-z0-9_-]+):(?:[ ]+(.*))?", line)
        if not field:
            return False, f"Unsupported or malformed frontmatter line: {line}"
        key, raw_value = field.group(1), field.group(2)
        if key in frontmatter:
            return False, f"Duplicate frontmatter key: {key}"
        if raw_value is None and key not in {"name", "description"}:
            index += 1
            while index < len(lines) and (not lines[index].strip() or lines[index][0].isspace()):
                index += 1
            frontmatter[key] = {}
            continue
        if raw_value is None:
            value: object = None
        else:
            scalar = raw_value.strip()
            if scalar in {"|", ">"}:
                block: list[str] = []
                index += 1
                while index < len(lines) and (not lines[index].strip() or lines[index][0].isspace()):
                    block.append(lines[index].lstrip())
                    index += 1
                frontmatter[key] = "\n".join(block)
                continue
            if len(scalar) >= 2 and scalar[0] == scalar[-1] and scalar[0] in {"'", '"'}:
                value = scalar[1:-1]
            elif scalar.lower() in {"null", "~", "true", "false", "yes", "no", "on", "off"}:
                value = None if scalar.lower() in {"null", "~"} else scalar.lower() in {"true", "yes", "on"}
            elif re.fullmatch(r"[-+]?(?:0|[1-9][0-9_]*)(?:\.[0-9_]+)?(?:[eE][-+]?[0-9]+)?", scalar):
                value = 0
            elif scalar.startswith("[") or scalar.startswith("{"):
                value = []
            else:
                value = scalar
        frontmatter[key] = value
        index += 1

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
