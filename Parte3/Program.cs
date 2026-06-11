// =============================================================================
// Parte 3 — Máquina de Turing (MT)
// Disciplina: Fundamentos Teóricos da Computação — Faculdade Cotemig 2026/1
//
// Linguagem-alvo L4 = { a^n b^n c^n | n ≥ 1 }
// Desafio:       f(n) = n + 1 em unário (incremento)
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MaquinaDeTuring
{
    // -------------------------------------------------------------------------
    // Representa uma transição da MT
    // δ : Q × Γ → Q × Γ × {L, R}
    // -------------------------------------------------------------------------
    class TransicaoMT
    {
        public string EstadoOrigem { get; }
        public char SimboloLido { get; }
        public string EstadoDestino { get; }
        public char SimboloEscrito { get; }
        public char Direcao { get; } // 'L' = esquerda, 'R' = direita

        public TransicaoMT(string origem, char lido,
                            string destino, char escrito, char direcao)
        {
            EstadoOrigem = origem;
            SimboloLido = lido;
            EstadoDestino = destino;
            SimboloEscrito = escrito;
            Direcao = direcao;
        }
    }

    // -------------------------------------------------------------------------
    // Máquina de Turing — 7-tupla: M = (Q, Σ, Γ, δ, q0, qaccept, qreject)
    // -------------------------------------------------------------------------
    class MaquinaTuring
    {
        // Q — conjunto de estados
        public HashSet<string> Q { get; private set; }

        // Σ — alfabeto de entrada (não contém branco '_')
        public HashSet<char> Sigma { get; private set; }

        // Γ ⊇ Σ — alfabeto da fita (contém '_' = branco ⊔)
        public HashSet<char> Gamma { get; private set; }

        // δ — função de transição como dicionário
        public Dictionary<(string estado, char simbolo),
                          (string novoEstado, char novoSimbolo, char direcao)> Delta
            { get; private set; }

        // q0 — estado inicial
        public string q0 { get; private set; }

        // qaccept — estado de aceitação
        public string qAccept { get; private set; }

        // qreject — estado de rejeição (distinto de qaccept)
        public string qReject { get; private set; }

        // Limite de passos configurável (evita loops infinitos)
        public int LimitePassos { get; set; } = 10_000;

        public string Nome { get; private set; }

        public MaquinaTuring(
            HashSet<string> q,
            HashSet<char> sigma,
            HashSet<char> gamma,
            Dictionary<(string, char), (string, char, char)> delta,
            string estadoInicial,
            string estadoAceitacao,
            string estadoRejeicao,
            string nome = "MT")
        {
            Q = q;
            Sigma = sigma;
            Gamma = gamma;
            Delta = delta;
            q0 = estadoInicial;
            qAccept = estadoAceitacao;
            qReject = estadoRejeicao;
            Nome = nome;
        }

        // -----------------------------------------------------------------
        // Simula a MT sobre a cadeia fornecida.
        // Retorna true se a MT alcança qaccept, false se alcança qreject.
        // Exibe cada configuração (estado, fita, posição do cabeçote).
        // -----------------------------------------------------------------
        public bool Simular(string cadeia, bool exibirPassos = true)
        {
            // Fita dinâmica: Dictionary<int,char>
            // Posição 0 = primeiro símbolo; posições negativas são blanks à esquerda
            var fita = new Dictionary<int, char>();
            for (int i = 0; i < cadeia.Length; i++)
                fita[i] = cadeia[i];

            string estadoAtual = q0;
            int cabecote = 0;
            int passo = 0;

            if (exibirPassos)
            {
                Console.WriteLine($"\n  [{Nome}] Entrada: \"{(cadeia == "" ? "ε" : cadeia)}\"");
                Console.WriteLine($"  {"Passo",-6} | {"Estado",-14} | Fita");
                Console.WriteLine($"  {new string('-', 60)}");
                ExibirFita(passo, estadoAtual, fita, cabecote);
            }

            while (passo < LimitePassos)
            {
                if (estadoAtual == qAccept)
                {
                    if (exibirPassos)
                    {
                        Console.WriteLine($"\n  ✅ Estado de aceitação '{qAccept}' alcançado em {passo} passos.");
                        ExibirFitaFinal(fita);
                    }
                    return true;
                }
                if (estadoAtual == qReject)
                {
                    if (exibirPassos)
                        Console.WriteLine($"\n  ❌ Estado de rejeição '{qReject}' alcançado em {passo} passos.");
                    return false;
                }

                char simboloAtual = fita.TryGetValue(cabecote, out char s) ? s : '_';
                var chave = (estadoAtual, simboloAtual);

                if (!Delta.TryGetValue(chave, out var transicao))
                {
                    // Sem transição definida → rejeita implicitamente
                    if (exibirPassos)
                        Console.WriteLine($"\n  ❌ Nenhuma transição para ({estadoAtual}, '{simboloAtual}'). Rejeitado.");
                    return false;
                }

                // Aplica a transição
                fita[cabecote] = transicao.novoSimbolo;
                estadoAtual = transicao.novoEstado;
                cabecote += transicao.direcao == 'R' ? 1 : -1;
                passo++;

                if (exibirPassos)
                    ExibirFita(passo, estadoAtual, fita, cabecote);
            }

            Console.WriteLine($"\n  [AVISO] Limite de {LimitePassos} passos atingido.");
            return false;
        }

        private static void ExibirFita(int passo, string estado,
            Dictionary<int, char> fita, int cabecote)
        {
            Console.Write($"  {passo,-6} | {estado,-14} | ");
            int min = fita.Count > 0 ? fita.Keys.Min() : 0;
            int max = fita.Count > 0 ? fita.Keys.Max() : 0;
            // Exibe alguns blanks extras em cada extremidade para clareza
            for (int i = min - 1; i <= max + 1; i++)
            {
                char c = fita.TryGetValue(i, out char v) ? v : '_';
                if (i == cabecote)
                    Console.Write($"[{c}]");
                else
                    Console.Write($" {c} ");
            }
            Console.WriteLine();
        }

        private static void ExibirFitaFinal(Dictionary<int, char> fita)
        {
            int min = fita.Count > 0 ? fita.Keys.Min() : 0;
            int max = fita.Count > 0 ? fita.Keys.Max() : 0;
            Console.Write("  Fita final: ");
            for (int i = min; i <= max; i++)
                Console.Write(fita.TryGetValue(i, out char v) ? v : '_');
            Console.WriteLine();
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
            Console.WriteLine("  FTC 2026/1 — Parte 3: Máquina de Turing");
            Console.WriteLine("=============================================================\n");

            // =================================================================
            // MT para L4 = { a^n b^n c^n | n ≥ 1 }
            // =================================================================
            //
            // 7-tupla:
            //   Q  = { q0, q1, q2, q3, q4, qback, qaccept, qreject }
            //   Σ  = { a, b, c }
            //   Γ  = { a, b, c, X, Y, Z, _ }   (_ = branco ⊔)
            //   q0 = q0
            //   qaccept = "qaccept"
            //   qreject = "qreject"
            //
            // Estratégia de marcação (varredura repetida):
            //   Cada iteração:
            //   1. q0: procura 'a' não marcado; marca com 'X'; move direita → q1
            //   2. q1: passa por a's e Y's; procura 'b' não marcado; marca com 'Y'; → q2
            //   3. q2: passa por b's e Z's; procura 'c' não marcado; marca com 'Z'; → qback
            //   4. qback: retrocede até encontrar 'X', volta ao q0 para próxima iteração
            //   5. Quando q0 encontra Y: todos os a's foram marcados.
            //      Verifica se não há b/c não marcados → aceita
            //
            // Após todas as marcações:
            //   - Fita deve conter: X*Y*Z* (sem a,b,c remanescentes)
            // =================================================================

            var mt = ConstruirMTL4();

            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("  PARTE 3A — L4 = { a^n b^n c^n | n ≥ 1 }");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            var testesL4 = new[]
            {
                ("abc",         true,  "n=1"),
                ("aabbcc",      true,  "n=2"),
                ("aaabbbccc",   true,  "n=3"),
                ("aabbc",       false, "2a,2b,1c"),
                ("ab",          false, "sem c"),
                ("abcabc",      false, "fora de ordem"),
                ("",            false, "ε"),
            };

            foreach (var (cadeia, esperado, justificativa) in testesL4)
            {
                Console.WriteLine($"\n  ─── Teste: \"{(cadeia==""?"ε":cadeia)}\" ({justificativa}) — Esperado: {(esperado?"ACEITA":"REJEITA")} ───");
                bool resultado = mt.Simular(cadeia, exibirPassos: true);
                string r = resultado ? "✅ ACEITA" : "❌ REJEITA";
                string ok = resultado == esperado ? "OK" : "FALHOU";
                Console.WriteLine($"  Resultado: {r} [{ok}]");
            }

            // =================================================================
            // DESAFIO: MT que computa f(n) = n + 1 em unário
            // Entrada: n ocorrências de '1'    Saída: n+1 ocorrências de '1'
            // =================================================================
            //
            // Estratégia simples:
            //   q0: percorre todos os '1's da esquerda para a direita → qfim
            //   qfim: ao encontrar branco '_', escreve '1' → qaccept
            // =================================================================

            Console.WriteLine("\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("  DESAFIO — MT f(n) = n + 1 (unário)");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            var mtIncremento = ConstruirMTIncremento();

            var testesIncremento = new[]
            {
                ("1",     "11"),
                ("111",   "1111"),
                ("11111", "111111"),
            };

            foreach (var (entrada, saída) in testesIncremento)
            {
                Console.WriteLine($"\n  ─── Entrada: \"{entrada}\" — Saída esperada: \"{saída}\" ───");
                mtIncremento.Simular(entrada, exibirPassos: true);
            }

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        // -----------------------------------------------------------------
        // Constrói a MT para L4 = { a^n b^n c^n | n ≥ 1 }
        // -----------------------------------------------------------------
        static MaquinaTuring ConstruirMTL4()
        {
            // δ codificada como dicionário conforme exigência do trabalho
            var delta = new Dictionary<(string, char), (string, char, char)>
            {
                // ── Estado q0: procura 'a' para marcar ──
                // Encontrou 'a': marca com X e vai procurar 'b'
                { ("q0", 'a'), ("q1", 'X', 'R') },
                // Passa por Y (a's já marcados de rodadas anteriores)
                { ("q0", 'Y'), ("q0", 'Y', 'R') },
                // Encontrou Y sem ter achado 'a' não marcado:
                // verificar se restam b ou c não marcados
                { ("q0", 'Z'), ("qcheck", 'Z', 'R') },
                // Branco no início → rejeita (cadeia vazia)
                { ("q0", '_'), ("qreject", '_', 'R') },
                // 'b' ou 'c' antes de 'a' ou 'Y' → rejeita
                { ("q0", 'b'), ("qreject", 'b', 'R') },
                { ("q0", 'c'), ("qreject", 'c', 'R') },

                // ── Estado q1: procura 'b' para marcar ──
                { ("q1", 'a'), ("q1", 'a', 'R') },
                { ("q1", 'Y'), ("q1", 'Y', 'R') },
                { ("q1", 'b'), ("q2", 'Y', 'R') },
                { ("q1", 'X'), ("qreject", 'X', 'R') },
                { ("q1", 'Z'), ("qreject", 'Z', 'R') },
                { ("q1", '_'), ("qreject", '_', 'R') },
                { ("q1", 'c'), ("qreject", 'c', 'R') },

                // ── Estado q2: procura 'c' para marcar ──
                { ("q2", 'b'), ("q2", 'b', 'R') },
                { ("q2", 'Z'), ("q2", 'Z', 'R') },
                { ("q2", 'c'), ("qback", 'Z', 'L') },
                { ("q2", '_'), ("qreject", '_', 'R') },
                { ("q2", 'a'), ("qreject", 'a', 'R') },
                { ("q2", 'Y'), ("q2", 'Y', 'R') },

                // ── Estado qback: retrocede até encontrar 'X' ──
                { ("qback", 'b'), ("qback", 'b', 'L') },
                { ("qback", 'Y'), ("qback", 'Y', 'L') },
                { ("qback", 'Z'), ("qback", 'Z', 'L') },
                { ("qback", 'a'), ("qback", 'a', 'L') },
                { ("qback", 'X'), ("q0", 'X', 'R') },

                // ── Estado qcheck: verifica que não sobram b/c não marcados ──
                { ("qcheck", 'Z'), ("qcheck", 'Z', 'R') },
                { ("qcheck", '_'), ("qaccept", '_', 'R') },
                // Se ainda há b ou c não marcados → rejeita
                { ("qcheck", 'b'), ("qreject", 'b', 'R') },
                { ("qcheck", 'c'), ("qreject", 'c', 'R') },
                { ("qcheck", 'Y'), ("qreject", 'Y', 'R') },
            };

            return new MaquinaTuring(
                q: new HashSet<string>
                    { "q0","q1","q2","qback","qcheck","qaccept","qreject" },
                sigma: new HashSet<char> { 'a', 'b', 'c' },
                gamma: new HashSet<char> { 'a', 'b', 'c', 'X', 'Y', 'Z', '_' },
                delta: delta,
                estadoInicial: "q0",
                estadoAceitacao: "qaccept",
                estadoRejeicao: "qreject",
                nome: "MT-L4"
            );
        }

        // -----------------------------------------------------------------
        // Constrói a MT para f(n) = n + 1 em unário
        // Entrada: 1^n    Saída: 1^(n+1)
        // -----------------------------------------------------------------
        static MaquinaTuring ConstruirMTIncremento()
        {
            // Estratégia:
            //   q0: percorre todos os '1' para a direita
            //   qfim: ao chegar no branco, escreve '1' e aceita

            var delta = new Dictionary<(string, char), (string, char, char)>
            {
                // Percorre os '1's existentes
                { ("q0", '1'), ("q0", '1', 'R') },
                // Encontra branco no final: escreve '1' adicional
                { ("q0", '_'), ("qaccept", '1', 'R') },
            };

            return new MaquinaTuring(
                q: new HashSet<string> { "q0", "qaccept", "qreject" },
                sigma: new HashSet<char> { '1' },
                gamma: new HashSet<char> { '1', '_' },
                delta: delta,
                estadoInicial: "q0",
                estadoAceitacao: "qaccept",
                estadoRejeicao: "qreject",
                nome: "MT-Incremento"
            );
        }
    }
}
