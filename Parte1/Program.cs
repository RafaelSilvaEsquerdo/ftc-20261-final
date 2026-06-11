// =============================================================================
// Parte 1 — Autômato Finito Determinístico (AFD)
// Disciplina: Fundamentos Teóricos da Computação — Faculdade Cotemig 2026/1
// Linguagem-alvo L1 = { w ∈ {a,b}* | w termina com "ab" }
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AFD
{
    // -------------------------------------------------------------------------
    // Representa a 5-tupla formal: M = (Q, Σ, δ, q0, F)
    // -------------------------------------------------------------------------
    class AutomatoFinitoDeterministico
    {
        // Q — conjunto finito de estados
        public HashSet<string> Q { get; private set; }

        // Σ — alfabeto finito de entrada
        public HashSet<char> Sigma { get; private set; }

        // δ : Q × Σ → Q — função de transição (dicionário)
        public Dictionary<(string estado, char simbolo), string> Delta { get; private set; }

        // q0 ∈ Q — estado inicial
        public string q0 { get; private set; }

        // F ⊆ Q — conjunto de estados de aceitação
        public HashSet<string> F { get; private set; }

        // Construtor que recebe a 5-tupla explicitamente
        public AutomatoFinitoDeterministico(
            HashSet<string> q,
            HashSet<char> sigma,
            Dictionary<(string, char), string> delta,
            string estadoInicial,
            HashSet<string> estadosAceitacao)
        {
            Q = q;
            Sigma = sigma;
            Delta = delta;
            q0 = estadoInicial;
            F = estadosAceitacao;
        }

        // -----------------------------------------------------------------
        // bool Aceitar(string cadeia)
        // Simula a leitura símbolo a símbolo e retorna true se aceita.
        // Também preenche 'rastro' com a sequência de estados percorridos.
        // -----------------------------------------------------------------
        public bool Aceitar(string cadeia, out List<string> rastro)
        {
            rastro = new List<string>();
            string estadoAtual = q0;
            rastro.Add(estadoAtual);

            foreach (char simbolo in cadeia)
            {
                // Verifica se o símbolo pertence ao alfabeto Σ
                if (!Sigma.Contains(simbolo))
                {
                    Console.WriteLine($"  [ERRO] Símbolo '{simbolo}' não pertence ao alfabeto Σ. Cadeia rejeitada.");
                    rastro.Add("ERRO");
                    return false;
                }

                var chave = (estadoAtual, simbolo);

                // Verifica se δ é total — trata caso de transição indefinida
                if (!Delta.TryGetValue(chave, out string? proximoEstado))
                {
                    Console.WriteLine($"  [ERRO] δ({estadoAtual}, '{simbolo}') indefinida (δ não-total). Cadeia rejeitada.");
                    rastro.Add("MORTO");
                    return false;
                }

                estadoAtual = proximoEstado!;
                rastro.Add(estadoAtual);
            }

            return F.Contains(estadoAtual);
        }

        // -----------------------------------------------------------------
        // void ExibirDiagrama()
        // Imprime a tabela de transições do AFD no console.
        // -----------------------------------------------------------------
        public void ExibirDiagrama()
        {
            Console.WriteLine("\n╔══════════════════════════════════════╗");
            Console.WriteLine("║      TABELA DE TRANSIÇÕES DO AFD     ║");
            Console.WriteLine("╠══════════════════════════════════════╣");

            // Cabeçalho
            Console.Write("║ {0,-12} ", "Estado");
            foreach (char s in Sigma)
                Console.Write("│ {0,-8} ", s);
            Console.WriteLine("║");
            Console.WriteLine("║─────────────────────────────────────║");

            // Linhas da tabela
            foreach (string estado in Q)
            {
                // Marcadores: → = inicial, * = aceitação
                string marcador = "";
                if (estado == q0) marcador += "→";
                if (F.Contains(estado)) marcador += "*";
                Console.Write("║ {0,-12} ", $"{marcador}{estado}");

                foreach (char s in Sigma)
                {
                    string destino = Delta.TryGetValue((estado, s), out string? d) ? d! : "—";
                    Console.Write("│ {0,-8} ", destino);
                }
                Console.WriteLine("║");
            }
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.WriteLine("  Legenda: → = estado inicial  * = estado de aceitação\n");
        }
    }

    // -------------------------------------------------------------------------
    // Classes auxiliares para desserialização do afd.json
    // -------------------------------------------------------------------------
    class AfdConfig
    {
        public List<string> estados { get; set; } = new();
        public List<string> alfabeto { get; set; } = new();
        public string estadoInicial { get; set; } = "";
        public List<string> estadosAceitacao { get; set; } = new();
        public List<TransicaoConfig> transicoes { get; set; } = new();
    }

    class TransicaoConfig
    {
        public string origem { get; set; } = "";
        public string simbolo { get; set; } = "";
        public string destino { get; set; } = "";
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
            Console.WriteLine("  FTC 2026/1 — Parte 1: Autômato Finito Determinístico (AFD)");
            Console.WriteLine("  Linguagem L1 = { w ∈ {a,b}* | w termina com \"ab\" }");
            Console.WriteLine("=============================================================\n");

            // -----------------------------------------------------------------
            // Construção do AFD para L1 como a 5-tupla formal
            //
            //  Q  = { q0, q1, q2 }
            //  Σ  = { a, b }
            //  q0 = q0   (estado inicial — nenhum sufixo relevante lido)
            //  F  = { q2 }
            //
            //  Semântica dos estados:
            //  q0 — estado inicial; último símbolo lido não foi 'a' (ou cadeia vazia)
            //  q1 — último símbolo lido foi 'a' (prefixo "...a")
            //  q2 — últimos dois símbolos lidos foram "ab" → estado de aceitação
            //
            //  Tabela δ:
            //        | a  | b
            //   ─────┼────┼────
            //   q0   | q1 | q0
            //   q1   | q1 | q2
            //   q2   | q1 | q0
            // -----------------------------------------------------------------

            var Q = new HashSet<string> { "q0", "q1", "q2" };
            var Sigma = new HashSet<char> { 'a', 'b' };
            var Delta = new Dictionary<(string, char), string>
            {
                { ("q0", 'a'), "q1" },
                { ("q0", 'b'), "q0" },
                { ("q1", 'a'), "q1" },
                { ("q1", 'b'), "q2" },
                { ("q2", 'a'), "q1" },
                { ("q2", 'b'), "q0" },
            };
            string estadoInicial = "q0";
            var F = new HashSet<string> { "q2" };

            var afd = new AutomatoFinitoDeterministico(Q, Sigma, Delta, estadoInicial, F);

            // Exibe a tabela de transições
            afd.ExibirDiagrama();

            // -----------------------------------------------------------------
            // Leitura do arquivo entradas.txt (um por linha)
            // -----------------------------------------------------------------
            string arquivoEntradas = "entradas.txt";
            if (!File.Exists(arquivoEntradas))
            {
                // Cria o arquivo com os casos de teste obrigatórios caso não exista
                File.WriteAllLines(arquivoEntradas, new[]
                {
                    "ab",
                    "aab",
                    "bab",
                    "ababab",
                    "ba",
                    "",           // cadeia vazia ε
                    "b",
                });
                Console.WriteLine($"Arquivo '{arquivoEntradas}' criado com casos de teste padrão.\n");
            }

            Console.WriteLine("=== PROCESSANDO entradas.txt ===\n");
            ProcessarEntradas(afd, arquivoEntradas);

            // -----------------------------------------------------------------
            // Desafio obrigatório: carrega AFD a partir de afd.json
            // -----------------------------------------------------------------
            string arquivoJson = "afd.json";
            if (!File.Exists(arquivoJson))
            {
                // Gera um afd.json de exemplo com o mesmo AFD de L1
                var config = new AfdConfig
                {
                    estados = new List<string> { "q0", "q1", "q2" },
                    alfabeto = new List<string> { "a", "b" },
                    estadoInicial = "q0",
                    estadosAceitacao = new List<string> { "q2" },
                    transicoes = new List<TransicaoConfig>
                    {
                        new TransicaoConfig { origem="q0", simbolo="a", destino="q1" },
                        new TransicaoConfig { origem="q0", simbolo="b", destino="q0" },
                        new TransicaoConfig { origem="q1", simbolo="a", destino="q1" },
                        new TransicaoConfig { origem="q1", simbolo="b", destino="q2" },
                        new TransicaoConfig { origem="q2", simbolo="a", destino="q1" },
                        new TransicaoConfig { origem="q2", simbolo="b", destino="q0" },
                    }
                };
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(arquivoJson, json);
                Console.WriteLine($"\nArquivo '{arquivoJson}' gerado como exemplo.\n");
            }

            Console.WriteLine("\n=== CARREGANDO AFD A PARTIR DE afd.json ===");
            var afdJson = CarregarAfdDeJson(arquivoJson);
            if (afdJson != null)
            {
                Console.WriteLine("AFD carregado com sucesso do JSON!\n");
                afdJson.ExibirDiagrama();
                Console.WriteLine("=== REPROCESSANDO entradas.txt com AFD do JSON ===\n");
                ProcessarEntradas(afdJson, arquivoEntradas);
            }

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }

        // -----------------------------------------------------------------
        // Processa cada linha do arquivo de entradas e exibe resultado
        // -----------------------------------------------------------------
        static void ProcessarEntradas(AutomatoFinitoDeterministico afd, string arquivo)
        {
            string[] linhas = File.ReadAllLines(arquivo);
            int numeroLinha = 0;

            foreach (string linha in linhas)
            {
                numeroLinha++;
                // Interpreta linha vazia como cadeia vazia ε
                string cadeia = linha;
                string exibicao = cadeia == "" ? "ε" : $"\"{cadeia}\"";

                Console.WriteLine($"─── Entrada #{numeroLinha}: {exibicao} ───");

                bool aceita = afd.Aceitar(cadeia, out List<string> rastro);

                // Exibe o rastro de estados
                Console.WriteLine($"  Rastro: {string.Join(" → ", rastro)}");
                string resultado = aceita ? "✅ ACEITA" : "❌ REJEITA";
                Console.WriteLine($"  Resultado: {resultado}");
                Console.WriteLine();
            }
        }

        // -----------------------------------------------------------------
        // Carrega e valida um AFD de um arquivo JSON
        // -----------------------------------------------------------------
        static AutomatoFinitoDeterministico? CarregarAfdDeJson(string caminho)
        {
            try
            {
                string json = File.ReadAllText(caminho);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<AfdConfig>(json, options);

                if (config == null)
                {
                    Console.WriteLine("[ERRO] Falha ao desserializar o JSON.");
                    return null;
                }

                // Validações obrigatórias
                if (string.IsNullOrWhiteSpace(config.estadoInicial))
                {
                    Console.WriteLine("[ERRO] Campo 'estadoInicial' ausente ou vazio.");
                    return null;
                }
                if (!config.estados.Contains(config.estadoInicial))
                {
                    Console.WriteLine("[ERRO] estadoInicial não está em 'estados'.");
                    return null;
                }
                foreach (var ea in config.estadosAceitacao)
                {
                    if (!config.estados.Contains(ea))
                    {
                        Console.WriteLine($"[ERRO] Estado de aceitação '{ea}' não está em 'estados'.");
                        return null;
                    }
                }

                // Monta a 5-tupla
                var Q = new HashSet<string>(config.estados);
                var Sigma = new HashSet<char>();
                foreach (var s in config.alfabeto)
                    if (s.Length == 1) Sigma.Add(s[0]);

                var Delta = new Dictionary<(string, char), string>();
                foreach (var t in config.transicoes)
                {
                    if (t.simbolo.Length != 1)
                    {
                        Console.WriteLine($"[AVISO] Símbolo '{t.simbolo}' na transição ignorado (deve ter 1 char).");
                        continue;
                    }
                    Delta[(t.origem, t.simbolo[0])] = t.destino;
                }

                var F = new HashSet<string>(config.estadosAceitacao);

                return new AutomatoFinitoDeterministico(Q, Sigma, Delta, config.estadoInicial, F);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO] Ao ler '{caminho}': {ex.Message}");
                return null;
            }
        }
    }
}
