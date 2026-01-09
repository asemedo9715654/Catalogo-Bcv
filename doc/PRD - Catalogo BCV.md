# PRD — Catálogo BCV (Data Warehouse)

## 1) Visão Geral
O Catálogo BCV é uma aplicação web em C# (.NET Core, MVC) para catalogação de bases de dados, com foco em Data Warehouse. O sistema permite registar ligações a múltiplas bases de dados, descrever campos, gerir aliases, gerir utilizadores e visualizar a estrutura catalogada em formato gráfico (centrada em tabelas facto e respetivas relações). Todas as ações relevantes devem ser auditáveis.

## 2) Objetivos do Produto
- Permitir catalogar várias bases de dados (DW e afins) a partir de credenciais fornecidas.
- Disponibilizar uma vista navegável do catálogo (tabelas, colunas e metadados).
- Suportar enriquecimento do metadado (descrições e aliases).
- Garantir auditabilidade completa das operações.
- Suportar gestão de utilizadores e controlo de acesso.
- Fornecer visualização gráfica por tabela facto e tabelas relacionadas.

## 3) Público-Alvo (Personas)
- Administrador do Catálogo
  - Gere utilizadores e permissões, define bases de dados a catalogar, supervisiona auditoria.
- Analista de Dados / BI
  - Consulta o catálogo, procura campos, usa descrições/aliases para entendimento do modelo.
- Engenheiro de Dados
  - Mantém o DW, valida metadados, atualiza descrições e aliases, acompanha alterações.

## 4) Problema e Benefícios
### Problema
- Metadados dispersos ou inexistentes dificultam entendimento e governação do DW.
- Falta de histórico/auditoria dificulta rastreabilidade de alterações no catálogo.
- Visualização do modelo (factos/dimensões/relacionamentos) é pouco acessível.

### Benefícios Esperados
- Melhoria do self-service para descoberta de dados.
- Redução de dependência de conhecimento tácito.
- Maior governança com auditoria e responsabilidades claras.

## 5) Escopo
### Incluído (MVP)
- Registo de ligações a bases de dados com:
  - Servidor, Base de dados, Utilizador, Palavra-passe.
- Adição/gestão de bases de dados a catalogar.
- Ingestão e persistência de metadados essenciais (tabelas e colunas).
- Descrição de campos (por coluna).
- Adição de aliases (por tabela e/ou coluna).
- Auditoria de ações (criação/edição/remoção e autenticação relevante).
- Adição/gestão de utilizadores.
- Visualização da base catalogada em modo gráfico:
  - Foco em tabela facto e tabelas relacionadas.

### Fora do Escopo (por agora)
- Integração com fontes não-SQL Server.
- Linhagem (data lineage) ponta-a-ponta (ETL/ELT).
- Catálogo de KPIs, relatórios e dashboards.
- Classificação avançada (tags, glossário corporativo, data quality scores).
- Workflows de aprovação para alterações de metadados.

## 6) Requisitos Funcionais
### RF-01 — Registar base de dados
- O utilizador autorizado consegue registar uma ligação a uma base de dados informando:
  - servidor, base de dados, utilizador, palavra passe.
- O sistema valida conectividade (sucesso/erro) antes de concluir o registo.

### RF-02 — Catalogar metadados
- Após registo, o sistema importa metadados mínimos:
  - lista de tabelas, colunas, tipos de dados (quando disponível), chaves e relações (quando possível).
- O catálogo suporta múltiplas bases de dados.

### RF-03 — Manter descrição de campos
- O utilizador autorizado consegue adicionar/editar descrição de:
  - colunas
  - (opcionalmente) tabelas
- O sistema mantém histórico via auditoria.

### RF-04 — Manter aliases
- O utilizador autorizado consegue adicionar/editar aliases para:
  - tabelas e/ou colunas
- O alias serve para busca e apresentação amigável.

### RF-05 — Gestão de utilizadores
- Administrador consegue:
  - criar utilizadores
  - ativar/desativar utilizadores
  - atribuir perfis/permissões (no mínimo: Administrador, Editor, Leitor)

### RF-06 — Auditoria
- O sistema regista eventos auditáveis, incluindo:
  - criação/edição/remoção de bases de dados catalogadas
  - criação/edição de descrições e aliases
  - gestão de utilizadores
  - ações de login/logout (se aplicável)
- Cada evento inclui:
  - quem (utilizador), quando (timestamp), o quê (ação), alvo (entidade), antes/depois (quando aplicável)

### RF-07 — Visualização gráfica por tabela facto
- O utilizador consegue selecionar uma base de dados catalogada e visualizar:
  - uma tabela facto e as tabelas relacionadas (dimensões e/ou relações relevantes)
- A visualização permite navegação (abrir detalhes de tabelas/colunas).

## 7) Requisitos Não Funcionais
### Segurança
- Palavra-passe de ligação à BD não deve ser armazenada em texto plano.
- Acesso à aplicação requer autenticação (mecanismo a definir no projeto) e autorização por perfil.
- A aplicação não deve expor credenciais nem em logs.

### Auditabilidade
- Auditoria deve ser imutável (apenas append) e consultável por administradores.

### Disponibilidade e Resiliência
- Falhas de conexão à BD de origem não devem corromper o catálogo.
- Operações de catalogação devem ser reexecutáveis.

### Performance (mínimos)
- Listagens e pesquisa de metadados devem responder em tempo aceitável para uso interativo.
- A visualização gráfica deve lidar com modelos típicos de DW (com limites práticos definidos na implementação).

### Stack Tecnológico (conforme requisitos)
- Linguagem: C#
- Framework: .NET Core
- Padrão: MVC
- Base de dados da aplicação: SQL Server (via Docker)

## 8) Modelo de Dados (alto nível)
Entidades esperadas (conceito):
- FonteDeDados/BaseDeDadosCatalogada (servidor, nome, estado, datas)
- Tabela (nome, schema, tipo: facto/dimensão/outro, metadados)
- Coluna (nome, tipo, nulável, etc.)
- Relacionamento (origem, destino, colunas, cardinalidade quando possível)
- Descrição (alvo: tabela/coluna, texto, autor, timestamps)
- Alias (alvo: tabela/coluna, texto, autor, timestamps)
- Utilizador (credenciais da aplicação, perfil, estado)
- EventoAuditoria (ação, alvo, antes/depois, utilizador, timestamps)

## 9) Fluxos Principais
### Fluxo A — Adicionar e catalogar uma base de dados
1. Administrador/Editor preenche servidor, base de dados, utilizador, palavra-passe.
2. Sistema valida ligação.
3. Sistema regista a base e executa catalogação inicial.
4. Sistema disponibiliza navegação de tabelas/colunas e auditoria do evento.

### Fluxo B — Enriquecer metadados (descrição/alias)
1. Editor abre uma tabela/coluna.
2. Editor adiciona ou edita descrição e/ou alias.
3. Sistema grava alteração e regista auditoria com antes/depois.

### Fluxo C — Visualização gráfica por tabela facto
1. Utilizador escolhe base catalogada.
2. Seleciona tabela facto.
3. Sistema apresenta grafo das relações e permite abrir detalhes.

## 10) Critérios de Aceitação (MVP)
- É possível registar pelo menos 1 base de dados com os 4 campos obrigatórios.
- Após registo, o catálogo lista tabelas e colunas dessa base.
- É possível adicionar/editar descrição de uma coluna e vê-la na consulta.
- É possível adicionar/editar alias e usá-lo na procura/apresentação.
- Existe gestão de utilizadores (criar e atribuir perfil mínimo).
- Todas as operações acima geram eventos de auditoria com quem/quando/o quê/alvo.
- Existe uma visualização gráfica por tabela facto e tabelas relacionadas.

## 11) Métricas de Sucesso
- Adoção: número de utilizadores ativos e consultas ao catálogo.
- Cobertura: número de bases catalogadas e percentagem de colunas com descrição/alias.
- Governança: percentagem de alterações com auditoria completa e consultável.

## 12) Riscos e Dependências
- Dependência da acessibilidade às bases de dados alvo e permissões do utilizador de ligação.
- Complexidade da extração de relações (nem sempre explícitas no DW).
- Definição objetiva de “tabela facto” pode exigir regras/configuração manual.

