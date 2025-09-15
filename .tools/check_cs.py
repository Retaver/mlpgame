#!/usr/bin/env python3
import os,sys,re
root='.'
problems=[]
cs_files=[]
for dirpath,dirs,files in os.walk(root):
    # skip hidden .git etc
    if '/.git' in dirpath: continue
    for f in files:
        if f.endswith('.cs'):
            cs_files.append(os.path.join(dirpath,f))

def check_file(path):
    with open(path,'r',encoding='utf-8',errors='ignore') as fh:
        s=fh.read()
    issues=[]
    # braces
    if s.count('{')!=s.count('}'):
        issues.append(f"unbalanced braces: {{ {s.count('{')} vs }} {s.count('}')}" )
    if s.count('(')!=s.count(')'):
        issues.append(f"unbalanced parens: ( {s.count('(')} vs ) {s.count(')')}")
    # quotes
    if s.count('"')%2!=0:
        issues.append('odd number of double quotes')
    if s.count("'")%2!=0:
        issues.append('odd number of single quotes')
    # TODOs
    todos=re.findall(r"TODO|FIXME", s)
    if todos:
        issues.append(f"contains TODO/FIXME ({len(todos)})")
    # dubious lambda-in-loop capture: look for for loops with clicked += () => ... i
    # simple heuristic: a for loop with 'for (int i =' and a '=> On' inside
    if re.search(r"for\s*\(.*int\s+i\s*=[^\)]*\).*\n[\s\S]{0,300}?=>", s):
        issues.append('possible closure capture in for-loop (check lambdas)')
    return issues

for f in cs_files:
    iss=check_file(f)
    if iss:
        problems.append((f,iss))

print('C# files scanned:', len(cs_files))
if not problems:
    print('No basic issues detected by this checker.')
    sys.exit(0)

print('\nPotential issues found:')
for p,iss in problems:
    print('\n--',p)
    for i in iss:
        print('   -',i)

# exit non-zero so we can programmatically detect
sys.exit(2)
