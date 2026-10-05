import os
import re

dir_path = r"C:\Users\mjvil\OneDrive\Escritorio\ERP_Osvaldo\ERP\src\ERP.Api\wwwroot"
index_path = os.path.join(dir_path, "index_erp.html")

with open(index_path, "r", encoding="utf-8") as f:
    index_content = f.read()

nav_match = re.search(r"<!-- NAVBAR RESPONSIVA UNIFICADA -->.*?<!-- FIN NAVBAR -->", index_content, re.DOTALL)
new_nav = nav_match.group(0)

html_files = [f for f in os.listdir(dir_path) if f.endswith(".html") and f != "login.html"]

for file in html_files:
    if file == "index_erp.html": continue
    file_path = os.path.join(dir_path, file)
    with open(file_path, "rb") as f:
        content_bytes = f.read()
    
    try:
        content = content_bytes.decode('utf-8')
    except:
        content = content_bytes.decode('latin-1')
    
    start_idx = content.find("<!-- NAVBAR RESPONSIVA UNIFICADA -->")
    
    end_idx_main = content.find("<main", start_idx)
    end_idx_root = content.find("<div id=\"root\"", start_idx)
    
    if end_idx_main == -1: end_idx = end_idx_root
    elif end_idx_root == -1: end_idx = end_idx_main
    else: end_idx = min(end_idx_main, end_idx_root)
    
    if start_idx != -1 and end_idx != -1:
        old_nav = content[start_idx:end_idx]
        content = content.replace(old_nav, new_nav + "\n\n    ")
    
    content = re.sub(r"window\.location\.pathname\.split\('/'\)\.pop\(\) \|\| 'index_erp\.html'", f"window.location.pathname.split('/').pop() || '{file}'", content)
    
    with open(file_path, "w", encoding='utf-8') as f:
        f.write(content)

print("Updated navbars successfully!")
