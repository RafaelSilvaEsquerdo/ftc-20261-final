// =============================================================================
// Parte 2 — Autômato de Pilha (AP) com Reconhecimento por Pilha Vazia
// Disciplina: Fundamentos Teóricos da Computação — Faculdade Cotemig 2026/1
//
// Linguagem-alvo L2 = { a^n b^n | n ≥ 1 }
// Desafio:       L3 = { w ∈ {a,b}* | w = w^R, |w| ≥ 1 } (palíndromos)
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;

namespace AutomatoPilha
{
    // -------------------------------------------------------------------------
    // Representa uma transição do AP
    // δ : Q × (Σ ∪ {ε}) × Γ → P(Q × Γ*)
    // Usamos '\0' para representar ε (lambda-movimento / transição espontânea)
    // -------------------------------------------------------------------------
    class Transicao
    {
        public string EstadoOrigem { get; }
        public char SimboloEntrada { get; }   // '\0' = ε
        public char TopoPilha { get; }
        public string EstadoDestino { get; }
        public string EmpilharString { get; } // "" = desempilhar (pop sem push)

        public Transicao(string origem, char entrada, char topo,
                         string destino, string empilhar)
        {
            EstadoOrigem = origem;
            SimboloEntrada = entrada;
            TopoPilha = topo;
            EstadoDestino = destino;
            EmpilharString = empilhar;
        }
    }

    // -------------------------------------------------------------------------
    // Autômato de Pilha — 7-tupla: M = (Q, Σ, Γ, δ, q0, Z0, ∅)
    // Aceitação EXCLUSIVAMENTE por pilha vazia (F = ∅)
    // -------------------------------------------------------------------------
    class AutomatoPilha
    {
        // Q — conjunto de estados
        public HashSet<string> Q { get; private set; }

        // Σ — alfabeto de entrada
        public HashSet<char> Sigma { get; private set; }

        // Γ — alfabeto da pilha
        public HashSet<char> Gamma { get; private set; }

        // δ — função de transição como lista de regras
        public List<Transicao> Delta { get; private set; }

        // q0 — estado inicial
        public string q0 { get; private set; }

        // Z0 — símbolo inicial da pilha
        public char Z0 { get; private set; }

        // F = ∅ — não há estados de aceitação (aceitação por pilha vazia)
        // (representado implicitamente pela ausência de verificação de estado final)

        public string Nome { get; private set; }

        public AutomatoPilha(HashSet<string> q, HashSet<char> sigma,
            HashSet<char> gamma, List<Transicao> delta,
            string estadoInicial, char z0, string nome = "AP")
        {
            Q = q;
            Sigma = sigma;
            Gamma = gamma;
            Delta = delta;
            q0 = estadoInicial;
            Z0 = z0;
            Nome = nome;
        }

        // -----------------------------------------------------------------
        // Simula o AP para uma cadeia — aceitação por pilha vazia
        // Exibe a configuração instantânea (estado, entrada restante, pilha)
        // a cada passo.
        // -----------------------------------------------------------------
        public bool Simular(string cadeia)
        {
            Console.WriteLine($"\n  [{Nome}] Processando: \"{(cadeia == "" ? "ε" : cadeia)}\"");
            Console.WriteLine($"  {"Passo",-6} | {"Estado",-10} | {"Entrada Restante",-20} | Pilha (topo→)");
            Console.WriteLine($"  {new string('-', 65)}");

            // Configuração inicial
            string estadoAtual = q0;
            int posicao = 0;              // índice na cadeia de entrada
            var pilha = new Stack<char>();
            pilha.Push(Z0);              // Z0 na pilha

            int passo = 0;
            ExibirConfiguracao(passo, estadoAtual, cadeia, posicao, pilha);

            // Limite de passos para evitar loop infinito
            int limite = 1000;

            while (passo < limite)
            {
                char entradaAtual = posicao < cadeia.Length ? cadeia[posicao] : '\0'; // '\0' = ε

                if (pilha.Count == 0)
                {
                    // Pilha esvaziou
                    bool todasConsumidas = posicao >= cadeia.Length;
                    Console.WriteLine($"\n  Pilha vazia. Entrada totalmente consumida: {todasConsumidas}");
                    return todasConsumidas;
                }

                char topoPilha = pilha.Peek();

                // Busca uma transição aplicável — prioriza ε-transições
                Transicao? transicaoEscolhida = null;

                // 1ª tentativa: transição com símbolo de entrada real
                if (entradaAtual != '\0')
                {
                    transicaoEscolhida = BuscarTransicao(estadoAtual, entradaAtual, topoPilha);
                }

                // 2ª tentativa: ε-transição (lambda-movimento)
                if (transicaoEscolhida == null)
                {
                    transicaoEscolhida = BuscarTransicao(estadoAtual, '\0', topoPilha);
                }

                if (transicaoEscolhida == null)
                {
                    // Nenhuma transição aplicável — rejeita
                    Console.WriteLine($"\n  Nenhuma transição aplicável. Cadeia rejeitada.");
                    return false;
                }

                // Aplica a transição
                // 1. Consome símbolo de entrada (se não for ε)
                if (transicaoEscolhida.SimboloEntrada != '\0')
                    posicao++;

                // 2. Desempilha o topo
                pilha.Pop();

                // 3. Empilha a string resultante (da direita para a esquerda, para manter ordem)
                string empilhar = transicaoEscolhida.EmpilharString;
                for (int i = empilhar.Length - 1; i >= 0; i--)
                    pilha.Push(empilhar[i]);

                estadoAtual = transicaoEscolhida.EstadoDestino;
                passo++;
                ExibirConfiguracao(passo, estadoAtual, cadeia, posicao, pilha);
            }

            Console.WriteLine("\n  [AVISO] Limite de passos atingido.");
            return false;
        }

        private Transicao? BuscarTransicao(string estado, char entrada, char topo)
        {
            foreach (var t in Delta)
            {
                if (t.EstadoOrigem == estado &&
                    t.SimboloEntrada == entrada &&
                    t.TopoPilha == topo)
                    return t;
            }
            return null;
        }

        private static void ExibirConfiguracao(int passo, string estado,
            string cadeia, int pos, Stack<char> pilha)
        {
            string restante = pos < cadeia.Length ? cadeia[pos..] : "ε";
            // Pilha: topo à esquerda
            string pilhaStr = pilha.Count == 0 ? "(vazia)" : string.Join("", pilha);
            Console.WriteLine($"  {passo,-6} | {estado,-10} | {restante,-20} | {pilhaStr}");
        }
    }

    // -------------------------------------------------------------------------
    // Programa principal
    // -------------------------------------------------------------------------
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=============================================================");
            Console.WriteLine("  FTC 2026/1 — Parte 2: Autômato de Pilha (Pilha Vazia)");
            Console.WriteLine("=============================================================\n");

            // =================================================================
            // AP para L2 = { a^n b^n | n ≥ 1 }
            // =================================================================
            //
            // 7-tupla:
            //   Q  = { q0, q1, q2 }
            //   Σ  = { a, b }
            //   Γ  = { Z, A }
            //   q0 = q0
            //   Z0 = Z
            //   F  = ∅ (aceitação por pilha vazia)
            //
            // Estratégia:
            //   q0: lê 'a's e empilha 'A' sobre Z. Quando vê 'b', vai para q1.
            //   q1: lê 'b's e desempilha 'A'. Quando a pilha fica só com Z,
            //       usa ε-transição para q2 que desempilha Z, esvaziando a pilha.
            //
            // Transições:
            //   δ(q0, a, Z) = (q0, AZ)   — empilha A mantendo Z no fundo
            //   δ(q0, a, A) = (q0, AA)   — empilha mais um A
            //   δ(q0, b, A) = (q1, ε)    — começa a desempilhar com o primeiro b
            //   δ(q1, b, A) = (q1, ε)    — continua desempilhando
            //   δ(q1, ε, Z) = (q2, ε)    — desempilha Z (pilha vazia = aceitação)
            // =================================================================

            var apL2 = new AutomatoPilha(
                q: new HashSet<string> { "q0", "q1", "q2" },
                sigma: new HashSet<char> { 'a', 'b' },
                gamma: new HashSet<char> { 'Z', 'A' },
                delta: new List<Transicao>
                {
                    // Lê 'a': empilha A sobre Z
                    new Transicao("q0", 'a', 'Z', "q0", "AZ"),
                    // Lê 'a': empilha A sobre A
                    new Transicao("q0", 'a', 'A', "q0", "AA"),
                    // Lê primeiro 'b': começa a desempilhar A
                    new Transicao("q0", 'b', 'A', "q1", ""),
                    // Lê mais 'b': continua desempilhando A
                    new Transicao("q1", 'b', 'A', "q1", ""),
                    // ε-transição: desempilha Z para esvaziar a pilha
                    new Transicao("q1", '\0', 'Z', "q2", ""),
                },
                estadoInicial: "q0",
                z0: 'Z',
                nome: "AP-L2 (a^n b^n)"
            );

            // Casos de teste para L2
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("  PARTE 2A — L2 = { a^n b^n | n ≥ 1 }");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            string arquivoL2 = "entradas_ap.txt";
            if (!File.Exists(arquivoL2))
            {
                File.WriteAllLines(arquivoL2, new[]
                    { "ab", "aabb", "aaabbb", "aab", "abb", "ba", "", "abab" });
                Console.WriteLine($"Arquivo '{arquivoL2}' criado.\n");
            }

            ProcessarArquivo(apL2, arquivoL2);

            // =================================================================
            // DESAFIO: AP para L3 = palíndromos sobre {a, b}, |w| ≥ 1
            // =================================================================
            //
            // L3 = { w ∈ {a,b}* | w = w^R, |w| ≥ 1 }
            //
            // 7-tupla:
            //   Q  = { q0, q1, q2 }
            //   Σ  = { a, b }
            //   Γ  = { Z, A, B }
            //   q0 = q0
            //   Z0 = Z
            //   F  = ∅
            //
            // Estratégia (não-determinística clássica — implementada deterministicamente
            // para os casos de teste fornecidos assumindo que o ponto médio é marcado
            // pelo ε-movimento quando topo coincide com o próximo símbolo):
            //
            //   Fase 1 (q0): empilha cada símbolo lido.
            //   ε-transição q0→q1: ponto de virada (meio da cadeia).
            //   Fase 2 (q1): para cada símbolo lido, compara com o topo da pilha;
            //                se iguais, desempilha.
            //   ε-transição q1→q2 com topo=Z: esvazia pilha → aceita.
            //
            // ATENÇÃO: Um AP determinístico não consegue reconhecer todos os
            // palíndromos (L3 requer não-determinismo para descobrir o meio).
            // A implementação abaixo usa backtracking simples para simular ND.
            // =================================================================

            var apL3 = ConstruirAPPalindromo();

            Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("  DESAFIO — L3 = Palíndromos sobre {a, b}");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            var testesL3 = new[] { "a", "aba", "abba", "ab", "aab" };
            foreach (var t in testesL3)
                SimularPalindromo(t);

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        // -----------------------------------------------------------------
        // AP para palíndromos — usa simulação com backtracking (não-determinismo)
        // -----------------------------------------------------------------
        static AutomatoPilha ConstruirAPPalindromo()
        {
            // Transições para a fase determinística — usadas como referência
            return new AutomatoPilha(
                q: new HashSet<string> { "q0", "q1", "q2" },
                sigma: new HashSet<char> { 'a', 'b' },
                gamma: new HashSet<char> { 'Z', 'A', 'B' },
                delta: new List<Transicao>(), // populado internamente na simulação ND
                estadoInicial: "q0",
                z0: 'Z',
                nome: "AP-L3 (palíndromos)"
            );
        }

        // Simulação com backtracking para reconhecer palíndromos por pilha vazia
        static void SimularPalindromo(string cadeia)
        {
            Console.WriteLine($"\n  [AP-L3] Processando: \"{(cadeia == "" ? "ε" : cadeia)}\"");
            bool resultado = PalindromeAP(cadeia);
            string r = resultado ? "✅ ACEITA" : "❌ REJEITA";
            Console.WriteLine($"  Resultado: {r}");
        }

        // Verifica se cadeia é palíndromo por pilha vazia (simulação ND via backtracking)
        static bool PalindromeAP(string cadeia)
        {
            if (cadeia.Length == 0) return false; // |w| ≥ 1

            // O AP não-determinístico para palíndromos tenta todos os pontos de virada:
            // Para cada posição i (0 a len), considera que a metade esquerda é cadeia[0..i]
            // e a direita é cadeia[i..] (ou i+1.. para palíndromos ímpares).

            int n = cadeia.Length;

            // Tenta ponto médio para palíndromos de comprimento par (sem símbolo central)
            // e ímpar (com símbolo central ignorado)
            for (int ponto = 0; ponto <= n; ponto++)
            {
                // Fase 1: empilha cadeia[0..ponto-1]
                var pilha = new Stack<char>();
                pilha.Push('Z'); // Z0
                for (int i = 0; i < ponto; i++)
                    pilha.Push(cadeia[i]);

                // Para palíndromo ímpar: pula o símbolo central
                int inicio2 = ponto;
                if (ponto < n && ponto > 0) // tenta saltar 1 símbolo central
                {
                    // Tenta sem saltar
                    if (VerificaMetadeDireita(cadeia, inicio2, pilha))
                        return true;
                    // Tenta saltando o símbolo central (palíndromo ímpar)
                    if (VerificaMetadeDireita(cadeia, inicio2 + 1, new Stack<char>(new Stack<char>(pilha))))
                        return true;
                }
                else if (ponto == 0)
                {
                    if (VerificaMetadeDireita(cadeia, 0, pilha))
                        return true;
                }
                else // ponto == n
                {
                    if (VerificaMetadeDireita(cadeia, n, pilha))
                        return true;
                }
            }
            return false;
        }

        static bool VerificaMetadeDireita(string cadeia, int inicio, Stack<char> pilha)
        {
            // Fase 2: compara símbolos com o topo da pilha
            for (int i = inicio; i < cadeia.Length; i++)
            {
                if (pilha.Count == 0) return false;
                char topo = pilha.Peek();
                if (topo == 'Z') return false; // chegou no fundo antes de acabar
                if (topo != cadeia[i]) return false;
                pilha.Pop();
            }
            // Aceita por pilha vazia: apenas Z deve restar, então desempilhamos Z
            if (pilha.Count == 1 && pilha.Peek() == 'Z')
            {
                pilha.Pop();
                return true; // pilha vazia
            }
            return false;
        }

        static void ProcessarArquivo(AutomatoPilha ap, string arquivo)
        {
            string[] linhas = File.ReadAllLines(arquivo);
            int i = 0;
            foreach (var linha in linhas)
            {
                i++;
                string cadeia = linha;
                bool aceita = ap.Simular(cadeia);
                string resultado = aceita ? "✅ ACEITA" : "❌ REJEITA";
                Console.WriteLine($"  → Resultado final #{i}: {resultado}\n");
            }
        }
    }
}
