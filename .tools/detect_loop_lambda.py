#!/usr/bin/env python3
import re,sys

def find_for_blocks(s):
    # naive: find 'for (' occurrences and extract the block that follows (brace-matched)
    res=[]
    form=re.finditer(r"for\s*\((.*?)\)", s, re.S)
    for m in form:
        start=m.end()
        # skip whitespace
        i=start
        while i<len(s) and s[i].isspace(): i+=1
        if i<len(s) and s[i]=='{':
            # find matching brace
            depth=0
            j=i
            while j<len(s):
                if s[j]=='{': depth+=1
                elif s[j]=='}': depth-=1
                j+=1
                if depth==0: break
            block=s[i:j]
            res.append((m.group(0), block))
        else:
            # single-statement loop - grab up to semicolon or newline
            j=i
            while j<len(s) and s[j]!=';': j+=1
            block=s[i:j+1]
            res.append((m.group(0), block))
    return res

if __name__=='__main__':
    if len(sys.argv)<2:
        print('usage: detect_loop_lambda.py file1.cs ...')
        sys.exit(1)
    for path in sys.argv[1:]:
        s=open(path,encoding='utf-8',errors='ignore').read()
        found=False
        for header,block in find_for_blocks(s):
            # only care about loops with int var (i or other)
            if re.search(r"int\s+\w+", header):
                if '=>' in block:
                    print(path+": closure-like lambda inside loop detected:\n  "+header)
                    found=True
        if not found:
            print(path+": no lambda detected inside for-loop blocks")
