# ftc-20261-final

**Disciplina:** Fundamentos Teóricos da Computação — Faculdade Cotemig 2026/1  
**Professor:** Júlio César da Silva  
**Data de entrega:** 10/06/2026

---

## Integrantes da Equipe

| Nome Completo | Matrícula |
|---|---|
| *Rafael Silva Esquerdo* | *72301295* |
| *Marcus Rizzon Melo* | *72500255* |

---

## Estrutura do Repositório

```
ftc-20261-final/
├── Parte1/          # AFD — Autômato Finito Determinístico (L1)
│   ├── Parte1.csproj
│   ├── Program.cs
│   ├── entradas.txt
│   └── afd.json
├── Parte2/          # Autômato de Pilha com aceitação por pilha vazia (L2 + L3)
│   ├── Parte2.csproj
│   ├── Program.cs
│   └── entradas_ap.txt
├── Parte3/          # Máquina de Turing (L4 + f(n)=n+1)
│   ├── Parte3.csproj
│   └── Program.cs
├── docs/
│   └── relatorio.pdf
├── .gitignore
└── README.md
```

---

## Descrição das Partes

### Parte 1 — AFD (2,5 pontos)
Implementação de um Autômato Finito Determinístico para a linguagem:

> **L1 = { w ∈ {a,b}\* | w termina com "ab" }**

- A 5-tupla `(Q, Σ, δ, q0, F)` é representada explicitamente em estruturas C# nomeadas.
- A função de transição `δ` é um `Dictionary<(string estado, char simbolo), string>`.
- O método `Aceitar(string cadeia)` simula leitura símbolo a símbolo e exibe o rastro de estados.
- O método `ExibirDiagrama()` imprime a tabela de transições no console.
- **Desafio:** carrega qualquer AFD a partir de `afd.json`.

### Parte 2 — Autômato de Pilha (3,0 pontos)
Implementação de dois APs com aceitação **exclusivamente por pilha vazia**:

- **L2 = { aⁿbⁿ | n ≥ 1 }** — AP determinístico com exibição de configuração instantânea a cada passo.
- **L3 = { w ∈ {a,b}\* | w = wᴿ, |w| ≥ 1 }** (palíndromos) — AP não-determinístico simulado com backtracking.

### Parte 3 — Máquina de Turing (3,0 pontos)
Implementação de duas MTs:

- **L4 = { aⁿbⁿcⁿ | n ≥ 1 }** — estratégia de marcação iterativa (X, Y, Z) com 7 estados, exibindo configuração a cada passo.
- **f(n) = n + 1 em unário** — dada fita com `n` ocorrências de `1`, produz `n+1` ocorrências.

---

## Como Compilar e Executar

### Pré-requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download) ou superior instalado.

### Parte 1 — AFD
```bash
cd Parte1
dotnet run
```
Os arquivos `entradas.txt` e `afd.json` são gerados automaticamente na primeira execução caso não existam.

### Parte 2 — Autômato de Pilha
```bash
cd Parte2
dotnet run
```
O arquivo `entradas_ap.txt` é gerado automaticamente na primeira execução.

### Parte 3 — Máquina de Turing
```bash
cd Parte3
dotnet run
```
Os casos de teste estão embutidos no código.

---

## Vídeo de Defesa

> 🎥 **Link do vídeo:** https://youtu.be/BBuJK25FlQA

---

## Referências

1. SIPSER, Michael. *Introduction to the Theory of Computation*. 3. ed. Boston: Cengage, 2013.
2. HOPCROFT, J. E.; MOTWANI, R.; ULLMAN, J. D. *Introdução à Teoria de Autômatos, Linguagens e Computação*. 2. ed. Rio de Janeiro: Campus/Elsevier, 2003.
3. MENEZES, Paulo Blauth. *Linguagens Formais e Autômatos*. 6. ed. Porto Alegre: Bookman, 2010.
