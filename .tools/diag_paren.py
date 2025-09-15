#!/usr/bin/env python3
import sys

def analyze(path):
    with open(path,'r',encoding='utf-8',errors='ignore') as f:
        lines=f.readlines()
    bal=0
    for i,l in enumerate(lines,1):
        for ch in l:
            if ch=='(':
                bal+=1
            elif ch==')':
                bal-=1
            if bal<0:
                print(f"{path}: negative balance at line {i}")
                return
    print(f"{path}: final balance {bal} (lines {len(lines)})")

if __name__=='__main__':
    if len(sys.argv)<2:
        print('Usage: diag_paren.py file1 [file2 ...]')
        sys.exit(1)
    for p in sys.argv[1:]:
        analyze(p)
