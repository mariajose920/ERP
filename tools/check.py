# Módulo: Transversal (T9, script de diagnóstico) — Responsable: Cristóbal
import os
import sys

file_path = os.path.join(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'src', 'ERP.Api', 'wwwroot'), 'pantalla_estados_financieros.html')  # ruta relativa
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

babel_start = content.find('<script type="text/babel">')
print(content[babel_start:babel_start+1000])
