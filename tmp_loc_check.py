import json 
import io 
LANGS = ['tr-TR','en-US','ar-SA'] 
with io.open('src/MerchantPortal/Resources/localization.json','r',encoding='utf-8') as f: 
    data = json.load(f) 
lang_sets = {} 
for lang in LANGS: 
    entries = data.get(lang) 
    if isinstance(entries, dict): 
        lang_sets[lang] = set(entries.keys()) 
    else: 
all_keys = set() 
for s in lang_sets.values(): 
    all_keys.update(s) 
has_missing = False 
for lang in LANGS: 
    keys = lang_sets.get(lang) 
    if not keys: 
        continue 
    missing = sorted(all_keys - keys) 
    if missing: 
        has_missing = True 
        print('Missing entries under {}: {}'.format(lang, len(missing))) 
        for key in missing: 
            print('  ' + key) 
if not has_missing: 
    print('All base language blocks aligned') 
print('\nChecking cross-language sections...') 
for key, value in data.items(): 
    if key in LANGS: 
        continue 
    if isinstance(value, dict): 
        present = {sub_key for sub_key in value.keys() if sub_key in LANGS} 
        if present and present != set(LANGS): 
            missing = set(LANGS) - present 
            print('  Section {} missing languages: {}'.format(key, ', '.join(sorted(missing)))) 
