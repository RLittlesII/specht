import json
import os
import shutil
import sys

root = sys.argv[1]
source = os.path.join(root, 'docs', 'rules', 'source')
index = json.load(open(os.path.join(source, 'rules.json'), encoding='utf-8'))
versions = index['versions']


def key(text):
    return tuple(int(part) for part in text.split('.'))


for version in versions:
    folder = os.path.join(root, 'docs', 'rules', 'v' + version)
    shutil.rmtree(folder, ignore_errors=True)
    os.makedirs(folder)
    for rule, entries in sorted(index['rules'].items()):
        for entry in entries:
            if key(entry['from']) <= key(version) <= key(entry.get('to', versions[-1])):
                text = open(os.path.join(source, entry['file']), encoding='utf-8', newline='').read()
                with open(os.path.join(folder, rule + '.md'), 'w', encoding='utf-8', newline='') as out:
                    out.write(text.replace('{{schemaVersion}}', version))
