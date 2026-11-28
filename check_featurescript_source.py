"""Conservative source checks; this is not an Onshape FeatureScript compiler."""
from collections import Counter
from pathlib import Path
import re


def check(path=Path(__file__).with_name('kreslingEngineering.fs')):
    source = path.read_text()
    assert source.startswith('FeatureScript 2144;\nimport(path : "onshape/std/common.fs", version : "2144.0");\nimport(path : "onshape/std/chamfertype.gen.fs", version : "2144.0");\n'), 'Missing or mismatched pinned standard-library imports'
    assert len(re.findall(r'^FeatureScript\s+\d+;', source, re.M)) == 1
    assert len(re.findall(r'^import\(', source, re.M)) == 2
    precondition = source.split('precondition\n{', 1)[1].split('\n}\n{', 1)[0]
    scalar = re.findall(r'\bis(?:Length|Angle|Integer|Real)\s*\(\s*\w+\.(\w+)\s*,', precondition)
    typed = re.findall(r'\b\w+\.(\w+)\s+is\s+\w+\s*;', precondition)
    duplicates = [name for name, count in Counter(scalar + typed).items() if count > 1]
    assert not duplicates, f'Duplicate feature parameter IDs: {duplicates}'
    scrubbed = re.sub(r'//[^\n]*|"(?:\\.|[^"\\])*"', '', source)
    assert not re.search(r'\b(?:const|var)\s+box\b', scrubbed), 'Reserved keyword box used as a variable name'
    assert not re.search(r'\.\s*(?:type|function|enum|import|export|const|var|if|else|for|return)\b', scrubbed), 'Reserved keyword after dot'
    stack = []
    pairs = {')': '(', ']': '[', '}': '{'}
    for token in scrubbed:
        if token in '([{':
            stack.append(token)
        elif token in ')]}':
            assert stack and stack.pop() == pairs[token], 'Unbalanced source delimiters'
    assert not stack
    definitions = set(re.findall(r'function\s+(k\w+)\(', source))
    calls = set(re.findall(r'\b(k[A-Z]\w*)\s*\(', source))
    assert calls <= definitions, f'Missing helper functions: {calls - definitions}'
    assert '"keepTools" : true' in source
    assert 'MODIFIER_ONLY_NOT_PRINT__crease_' in source
    assert 'definition = mergeMaps(K_MANUFACTURING_DEFAULTS, definition)' in source
    assert 'row.cellTwist' in source and 'hit["type"]' in source
    print(f'Source checks passed: {len(scalar + typed)} unique parameter IDs, {len(definitions)} complete helpers. Onshape compilation is still required.')


if __name__ == '__main__':
    check()
