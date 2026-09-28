"""Apply the verified in-place F11 window-title edit to both bundled core DLLs.

The replacement occupies the existing 24-byte string slot. No PE addresses,
section sizes, imports, exports, or code bytes are changed.
"""

from hashlib import sha256
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
ORIGINAL_SHA256 = "b295b76df64de50242d88b39de6b70bcb50391dbc4274783ea7d113ef53c9a79"
PATCHED_SHA256 = "53320dc32ed64726f202463f1f1b47b697add25ab153dd9db625d3507b35e1c2"
OLD_SLOT = b"FulGer GOG Options" + b"\0" * 6
NEW_SLOT = b"FulGer Patch Options" + b"\0" * 4
assert len(OLD_SLOT) == len(NEW_SLOT) == 24


def patch(path: Path) -> None:
    data = path.read_bytes()
    digest = sha256(data).hexdigest()
    if digest == PATCHED_SHA256:
        print(f"Already patched: {path}")
        return
    if digest != ORIGINAL_SHA256:
        raise ValueError(f"Unexpected gog_core.dll build: {path}")
    if data.count(OLD_SLOT) != 1:
        raise ValueError(f"Expected exactly one F11 title slot: {path}")
    updated = data.replace(OLD_SLOT, NEW_SLOT, 1)
    if sha256(updated).hexdigest() != PATCHED_SHA256:
        raise ValueError(f"Unexpected patched core hash: {path}")
    path.write_bytes(updated)
    print(f"Patched: {path}")


for game in ("Warrior Within", "The Two Thrones"):
    patch(ROOT / game / "Payload" / "gog_core.dll")
