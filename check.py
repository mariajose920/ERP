import sys

file_path = r'C:\Users\mjvil\OneDrive\Escritorio\ERP_Osvaldo\ERP\src\ERP.Api\wwwroot\pantalla_estados_financieros.html'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

babel_start = content.find('<script type="text/babel">')
print(content[babel_start:babel_start+1000])
