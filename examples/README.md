# Exemplos

## `demo.gmt`

Macro de demonstração gerada por:

```powershell
Pulso.exe --demo examples/demo.gmt
```

Ela move o mouse, clica e digita `pulso`. Serve para testar reprodução,
edição na linha do tempo e compilação:

```powershell
Pulso.exe examples/demo.gmt
Pulso.exe --compile examples/demo.gmt demo.exe
```

> A macro usa coordenadas da tela em que foi gerada. Se você abrir em outra
> resolução, edite os eventos na linha do tempo ou grave a sua própria.
