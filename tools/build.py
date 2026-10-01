#!/usr/bin/env python3
"""Compile using installed game references. Does not download or deploy anything."""
import argparse
import json
import pathlib
import shutil
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--game-managed", type=pathlib.Path, required=True)
    parser.add_argument("--loader-dll", type=pathlib.Path, required=True)
    parser.add_argument("--content-dll", type=pathlib.Path, required=True)
    parser.add_argument("--harmony-dll", type=pathlib.Path, required=True)
    args = parser.parse_args()
    root = pathlib.Path(__file__).resolve().parents[1]
    references = list(args.game_managed.glob("*.dll"))
    if not (args.game_managed / "Assembly-CSharp.dll").is_file():
        parser.error("--game-managed must contain the installed game's Assembly-CSharp.dll")
    for path in (args.loader_dll, args.content_dll, args.harmony_dll):
        if not path.is_file():
            parser.error(f"Missing reference: {path}")
        references = [reference for reference in references if reference.name.casefold() != path.name.casefold()]
        references.append(path)
    dotnet = shutil.which("dotnet")
    if not dotnet:
        parser.error("Install .NET SDK 8 or newer; dotnet was not found")
    sdks = subprocess.check_output([dotnet, "--list-sdks"], text=True).splitlines()
    compilers = []
    for line in sdks:
        version, _, directory = line.partition(" [")
        try:
            major = int(version.split(".")[0])
        except ValueError:
            continue
        compiler = pathlib.Path(directory.rstrip("]")) / version / "Roslyn" / "bincore" / "csc.dll"
        if major >= 8 and compiler.is_file():
            compilers.append(compiler)
    if not compilers:
        parser.error("No .NET SDK 8+ C# compiler found (a runtime alone is insufficient)")
    out = root / "build" / "Overmind"
    out.mkdir(parents=True, exist_ok=True)
    dll = out / "Overmind.dll"
    # Remove stale output before compiling so a failure cannot look like a new build.
    dll.unlink(missing_ok=True)
    command = [dotnet, str(compilers[-1]), "-target:library", "-nostdlib", "-noconfig", "-langversion:latest", f"-out:{dll}"]
    command.extend(f"-r:{p.resolve()}" for p in dict.fromkeys(references))
    command.extend(str(p) for p in sorted((root / "Overmind").rglob("*.cs")))
    result = subprocess.run(command, check=False)
    if result.returncode:
        dll.unlink(missing_ok=True)
        return result.returncode
    json.loads((root / "Overmind" / "mod.json").read_text())
    shutil.copy2(root / "Overmind" / "mod.json", out / "mod.json")
    print(f"Built {dll}. Validate patch targets and gameplay before installation.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
