#!/usr/bin/env python3
import os,sys,re

def strip_strings_and_comments(s):
    # remove block comments
    s = re.sub(r"/\*[\s\S]*?\*/", lambda m: ' ' * (m.end()-m.start()), s)
    # remove line comments
    s = re.sub(r"//.*", lambda m: ' ' * (m.end()-m.start()), s)
    # remove double-quoted strings (handles escaped quotes)
    s = re.sub(r'"(?:\\.|[^"\\])*"', lambda m: '"' + ' '*(m.end()-m.start()-2) + '"', s)
    # remove char literals
    s = re.sub(r"'(?:\\.|[^'\\])+'", lambda m: "'" + ' '*(m.end()-m.start()-2) + "'", s)
    return s

def check_file(path):
    with open(path,'r',encoding='utf-8',errors='ignore') as fh:
        s=fh.read()
    stripped = strip_strings_and_comments(s)
    issues=[]
    if stripped.count('{')!=stripped.count('}'):
        issues.append(f"unbalanced braces: {{ {stripped.count('{')} vs }} {stripped.count('}')}" )
    if stripped.count('(')!=stripped.count(')'):
        issues.append(f"unbalanced parens: ( {stripped.count('(')} vs ) {stripped.count(')')}")
    # quotes in original source (strings/chars) - count in original because stripping removes them
    if s.count('"')%2!=0:
        issues.append('odd number of double quotes')
    # Note: do not flag odd single quotes (apostrophes) in comments/docs; they're common and harmless.
    todos=re.findall(r"TODO|FIXME", s)
    if todos:
        issues.append(f"contains TODO/FIXME ({len(todos)})")
    # closure-in-loop heuristic: look for for-loops that declare an int loop variable
    # and then contain a lambda (=>) that references that same loop variable name.
    for_match_iter = re.finditer(r"for\s*\(([^)]*)\)", s, re.S)
    for m in for_match_iter:
        header = m.group(1)
        var_match = re.search(r"int\s+(\w+)", header)
        if not var_match: continue
        var_name = var_match.group(1)
        # attempt to extract block following the for(...) — naive: take 1000 chars after header
        start = m.end()
        block = s[start:start+2000]
        # If a lambda appears in the block and references the loop variable by name, flag it
        if '=>' in block and re.search(r"\b" + re.escape(var_name) + r"\b", block):
            issues.append(f'possible closure capture in for-loop (check lambdas referencing "{var_name}")')
            break
    return issues

def main():
    root='.'
    problems=[]
    cs_files=[]
    for dirpath,dirs,files in os.walk(root):
        if '/.git' in dirpath: continue
        for f in files:
            if f.endswith('.cs'):
                cs_files.append(os.path.join(dirpath,f))

    for f in cs_files:
        iss=check_file(f)
        if iss:
            problems.append((f,iss))

    print('C# files scanned:', len(cs_files))
    if not problems:
        print('No basic issues detected by this checker.')
        return 0
    print('\nPotential issues found:')
    for p,iss in problems:
        print('\n--',p)
        for i in iss:
            print('   -',i)
    return 2

if __name__=='__main__':
    sys.exit(main())
