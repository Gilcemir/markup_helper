# **Guia para o uso de elementos e atributos XML em documentos que seguem a implementação SciELO Publishing Schema** {#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema}

# **e outras informações relevantes à publicação** {#e-outras-informações-relevantes-à-publicação}

*Idioma: Português*  
Última atualização: 22/05/2025  
![Imagem decorativa: Logo SciELO Publishing Schema][image1]

[Guia para o uso de elementos e atributos XML em documentos que seguem a implementação SciELO Publishing Schema](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema)

[e outras informações relevantes à publicação](#e-outras-informações-relevantes-à-publicação)

[🔹INTRODUÇÃO](#🔹introdução)

[🔹VERSÃO](#🔹versão)

[🔹CONVENÇÕES UTILIZADAS NESTE GUIA](#🔹convenções-utilizadas-neste-guia)

[🔹SUPORTE](#🔹suporte)

[🔹DOCUMENTOS INDEXÁVEIS E NÃO INDEXÁVEIS](#🔹documentos-indexáveis-e-não-indexáveis)

[Documentos Indexáveis](#documentos-indexáveis)

[Documentos Não Indexáveis](#documentos-não-indexáveis)

[Equivalência entre documentos indexáveis e @article-type em \<article\>](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>)

[🔹ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis)

[Excepcionalidade para os 6 elementos obrigatórios para publicação](#excepcionalidade-para-os-6-elementos-obrigatórios-para-publicação)

[Excepcionalidade para documentos Retrospectivos](#excepcionalidade-para-documentos-retrospectivos)

[🔹MODALIDADES DE PUBLICAÇÃO ACEITAS](#🔹modalidades-de-publicação-aceitas)

[Características da Publicação Regular](#características-da-publicação-regular)

[Características da Publicação Contínua](#características-da-publicação-contínua)

[Antecipação e Restrições de Publicação na Modalidade Contínua](#antecipação-e-restrições-de-publicação-na-modalidade-contínua)

[🔹ENTREGA DE PACOTE XML PARA PUBLICAÇÃO](#🔹entrega-de-pacote-xml-para-publicação)

[O Que é um Pacote de Entrega](#o-que-é-um-pacote-de-entrega)

[O que Contém um Pacote de Entrega](#o-que-contém-um-pacote-de-entrega)

[Formatos Permitidos para os Arquivos que Compõem o Pacote de Entrega](#formatos-permitidos-para-os-arquivos-que-compõem-o-pacote-de-entrega)

[Acrônimos Oficiais dos Periódicos](#acrônimos-oficiais-dos-periódicos)

[Regras para Nomeação de Arquivos e Pastas](#regras-para-nomeação-de-arquivos-e-pastas)

[Nomeação de Arquivos](#nomeação-de-arquivos)

[Nomeação de Pastas](#nomeação-de-pastas)

[Planilha de Other: Controle de Seções](#planilha-de-other:-controle-de-seções)

[Como Realizar a Entrega de Artigos para Publicação](#como-realizar-a-entrega-de-artigos-para-publicação)

[Depósito do Pacote de Entrega no FTP](#depósito-do-pacote-de-entrega-no-ftp)

[Informação do Depósito do Pacote de Entrega Via Email](#informação-do-depósito-do-pacote-de-entrega-via-email)

[Composição para o Título do Email de Entrega](#composição-para-o-título-do-email-de-entrega)

[Composição do Corpo do Email de Entrega](#composição-do-corpo-do-email-de-entrega)

[🔹FLUXO DE PUBLICAÇÃO DE DOCUMENTOS](#🔹fluxo-de-publicação-de-documentos)

[Status padronizados dos emails](#status-padronizados-dos-emails)

[Tabela-síntese de status do fluxo de publicação](#tabela-síntese-de-status-do-fluxo-de-publicação)

[🔹TEMPO DE PUBLICAÇÃO](#🔹tempo-de-publicação)

[Cronograma de Processamento de Pacotes para Publicação](#cronograma-de-processamento-de-pacotes-para-publicação)

[🔹Encoding e \<\!DOCTYPE\>](#🔹encoding-e-\<!doctype\>)

[🔹CODIFICAÇÃO DE CARACTERES ESPECIAIS](#🔹codificação-de-caracteres-especiais)

[🔹MARCAÇÃO PARA ACESSIBILIDADE](#🔹marcação-para-acessibilidade)

[\<alt-text\>](#\<alt-text\>)

[\<long-desc\>](#\<long-desc\>)

[\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>)

[\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada)

[🔹LISTA DE MARCAÇÃO](#🔹lista-de-marcação)

[Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id)

[\<abstract\>: Resumo, Highlights, Visual Abstract e In Brief](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief)

[\<aff\>: Afiliação de Autores](#\<aff\>:-afiliação-de-autores)

[\<alternatives\>: .svg](#\<alternatives\>:-.svg)

[\<app-group\>: Apêndice e Anexo](#\<app-group\>:-apêndice-e-anexo)

[\<article\>: Artigo](#\<article\>:-artigo)

[\<article-categories\>: Seção de Documento](#\<article-categories\>:-seção-de-documento)

[\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other)

[Outros usos para número other](#outros-usos-para-número-other)

[\<contrib-group\>: Autoria](#\<contrib-group\>:-autoria)

[\<contrib\>: \<name\> e \<collab\>](#\<contrib\>:-\<name\>-e-\<collab\>)

[\<contrib-id\>: ORCID](#\<contrib-id\>:-orcid)

[\<role\>: Papel do Autor \- Taxonomia CRediT](#\<role\>:-papel-do-autor---taxonomia-credit)

[\<disp-formula\> e \<inline-formula\>: Equação e Fórmula Codificada](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada)

[\<ext-link\>: link](#\<ext-link\>:-link)

[\<fig\>: Figura](#\<fig\>:-figura)

[\<fn\>: Nota de Autor, Documento e Tabela](#\<fn\>:-nota-de-autor,-documento-e-tabela)

[\<author-notes\> \+ \<fn\>: Notas de Autor](#\<author-notes\>-+-\<fn\>:-notas-de-autor)

[\<fn-group\> \+ \<fn\>: Notas de Documento](#\<fn-group\>-+-\<fn\>:-notas-de-documento)

[\<table-wrap-foot\> \+ \<fn\>: Notas de Tabela](#\<table-wrap-foot\>-+-\<fn\>:-notas-de-tabela)

[\<funding-group\>: Financiamento e Apoio](#\<funding-group\>:-financiamento-e-apoio)

[\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura)

[\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico)

[\<issue\>: Número, Número Especial e Suplemento](#\<issue\>:-número,-número-especial-e-suplemento)

[\<journal-meta\>: Metadados do Periódico](#\<journal-meta\>:-metadados-do-periódico)

[\<list\>: Lista](#\<list\>:-lista)

[\<media\> e \<inline-media\>: Objeto Multimídia](#\<media\>-e-\<inline-media\>:-objeto-multimídia)

[\<permissions\>: Licença Creative Commons e Copyright](#\<permissions\>:-licença-creative-commons-e-copyright)

[\<product\>: Resenha de Livro](#\<product\>:-resenha-de-livro)

[\<pub-date\>: Datas de Publicação](#\<pub-date\>:-datas-de-publicação)

[\<ref-list\>: Lista de Referências](#\<ref-list\>:-lista-de-referências)

[\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos)

[\<response\>: Conjunto de Respostas](#\<response\>:-conjunto-de-respostas)

[\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto)

[\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo)

[\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar)

[\<table-wrap\>: Tabela](#\<table-wrap\>:-tabela)

[\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento)

[\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada)

[🔹LISTA DE MARCAÇÕES ESPECÍFICAS](#🔹lista-de-marcações-específicas)

[Adendo](#adendo)

[XML do Adendo](#xml-do-adendo)

[XML do Documento Mencionado pelo Adendo](#xml-do-documento-mencionado-pelo-adendo)

[Carta](#carta)

[XML da Carta](#xml-da-carta)

[XML da Carta com Resposta](#xml-da-carta-com-resposta)

[XML do Documento Mencionado pela Carta](#xml-do-documento-mencionado-pela-carta)

[XML da Resposta para uma Carta](#xml-da-resposta-para-uma-carta)

[Comentário](#comentário)

[XML do Comentário](#xml-do-comentário)

[XML do Comentário com Resposta](#xml-do-comentário-com-resposta)

[XML do Documento Comentado](#xml-do-documento-comentado)

[XML da Resposta para um Comentário](#xml-da-resposta-para-um-comentário)

[Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados)

[Exemplos textuais para declaração de disponibilidade de dados de documentos publicados na coleção SciELO Brasil:](#exemplos-textuais-para-declaração-de-disponibilidade-de-dados-de-documentos-publicados-na-coleção-scielo-brasil:)

[@data-available : Dados Disponíveis](#@data-available-:-dados-disponíveis)

[@data-available-upon-request : Dados Disponíveis Mediante Solicitação](#@data-available-upon-request-:-dados-disponíveis-mediante-solicitação)

[@uninformed : Dados não Informados / Não Utilizou Dados](#@uninformed-:-dados-não-informados-/-não-utilizou-dados)

[@data-not-available : Dados não Disponíveis](#@data-not-available-:-dados-não-disponíveis)

[@data-in-article : Dados no Artigo](#@data-in-article-:-dados-no-artigo)

[Declaração de Editor Responsável pelo Processo de Avaliação](#declaração-de-editor-responsável-pelo-processo-de-avaliação)

[Ensaio Clínico](#ensaio-clínico)

[Errata](#errata)

[XML da Errata](#xml-da-errata)

[XML do Documento Mencionado pela Errata](#xml-do-documento-mencionado-pela-errata)

[Manifestação de Preocupação](#manifestação-de-preocupação)

[XML da Manifestação de Preocupação](#xml-da-manifestação-de-preocupação)

[XML do Documento Mencionado pela Manifestação de Preocupação](#xml-do-documento-mencionado-pela-manifestação-de-preocupação)

[Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta)

[Preprint: Documentos Publicados Anteriormente como Preprint](#preprint:-documentos-publicados-anteriormente-como-preprint)

[Retratação](#retratação)

[XML da Retratação Total e Parcial](#xml-da-retratação-total-e-parcial)

[XML do Documento Retratado Totalmente](#xml-do-documento-retratado-totalmente)

[XML do Documento Retratado Parcialmente](#xml-do-documento-retratado-parcialmente)

\----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)**Seções com especificidades dos Critérios SciELO Brasil**

[🔹DOCUMENTOS INDEXÁVEIS E NÃO INDEXÁVEIS](#🔹documentos-indexáveis-e-não-indexáveis)

[Documentos Indexáveis](#documentos-indexáveis)

[Documentos Não Indexáveis](#documentos-não-indexáveis)

[Equivalência entre documentos indexáveis e @article-type em \<article\>](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>)

[🔹ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis)

[🔹LISTA DE MARCAÇÃO](#🔹lista-de-marcação)

[\<aff\>: Afiliação de Autores](#\<aff\>:-afiliação-de-autores)

[\<article\>: Artigo](#\<article\>:-artigo)

[\<article-categories\>: Seção de Documento](#\<article-categories\>:-seção-de-documento)

[\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other)

[\<contrib-group\>: Autoria](#\<contrib-group\>:-autoria)

[\<contrib\>: \<name\> e \<collab\>](#\<contrib\>:-\<name\>-e-\<collab\>)

[\<contrib-id\>: ORCID](#\<contrib-id\>:-orcid)

[\<role\>: Papel do Autor \- Taxonomia CRediT](#\<role\>:-papel-do-autor---taxonomia-credit)

[\<permissions\>: Licença Creative Commons e Copyright](#\<permissions\>:-licença-creative-commons-e-copyright)

[\<ref-list\>: Lista de Referências](#\<ref-list\>:-lista-de-referências)

[\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo)

[\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento)

[\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada)

[🔹LISTA DE MARCAÇÕES ESPECÍFICAS](#🔹lista-de-marcações-específicas)

[Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados)

[Exemplos textuais para declaração de disponibilidade de dados de documentos publicados na coleção SciELO Brasil:](#exemplos-textuais-para-declaração-de-disponibilidade-de-dados-de-documentos-publicados-na-coleção-scielo-brasil:)

[@data-available : Dados Disponíveis](#@data-available-:-dados-disponíveis)

[@data-available-upon-request : Dados Disponíveis Mediante Solicitação](#@data-available-upon-request-:-dados-disponíveis-mediante-solicitação)

[@uninformed : Dados não Informados / Não Utilizou Dados](#@uninformed-:-dados-não-informados-/-não-utilizou-dados)

[@data-not-available : Dados não Disponíveis](#@data-not-available-:-dados-não-disponíveis)

[@data-in-article : Dados no Artigo](#@data-in-article-:-dados-no-artigo)

[Declaração de Editor Responsável pelo Processo de Avaliação](#declaração-de-editor-responsável-pelo-processo-de-avaliação)

[Ensaio Clínico](#ensaio-clínico)

[Errata](#errata)

[Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta)

[Preprint: Documentos Publicados Anteriormente como Preprint](#preprint:-documentos-publicados-anteriormente-como-preprint)

[Retratação](#retratação)

\----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]**Seções com boas práticas de acessibilidade**

[🔹CODIFICAÇÃO DE CARACTERES ESPECIAIS](#🔹codificação-de-caracteres-especiais)

[🔹MARCAÇÃO PARA ACESSIBILIDADE](#🔹marcação-para-acessibilidade)

[\<alt-text\>](#\<alt-text\>)

[\<long-desc\>](#\<long-desc\>)

[\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>)

[\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada)

[🔹LISTA DE MARCAÇÃO](#🔹lista-de-marcação)

[\<alternatives\>: .svg](#\<alternatives\>:-.svg)

[\<app-group\>: Apêndice e Anexo](#\<app-group\>:-apêndice-e-anexo)

[\<disp-formula\> e \<inline-formula\>: Equação e Fórmula Codificada](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada)

[\<ext-link\>: link](#\<ext-link\>:-link)

[\<fig\>: Figura](#\<fig\>:-figura)

[\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura)

[\<list\>: Lista](#\<list\>:-lista)

[\<media\> e \<inline-media\>: Objeto Multimídia](#\<media\>-e-\<inline-media\>:-objeto-multimídia)

[\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto)

\----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

# **🔹INTRODUÇÃO** {#🔹introdução}

Este guia descreve o uso do estilo de marcação adotado pelo SciELO para submissão de documentos em formato XML.

O SciELO Publishing Schema ([SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema)) é composto pelas especificações:

1. [NISO JATS Journal Publishing DTD 1.3](https://jats.nlm.nih.gov/publishing/tag-library/1.3/);  
2. [Recomendações JATS4R](https://jats4r.org/recommendations/) (quando aplicável);  
3. Estilo [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema) com regras específicas da Metodologia SciELO.

A marcação XML neste guia documenta dados que requerem obrigatoriamente **elementos**, **atributos e/ou valores** que na JATS são opcionais. Em alguns casos, define o **conteúdo** de marcação para elementos. Usualmente as obrigatoriedades tem relação direta com os Critérios de indexação das coleções SciELO. Por este motivo, os usuários deste guia devem possuir conhecimentos prévios de XML, DTD, JATS e [Critérios, política e procedimentos para a](https://www.scielo.br/about/criterios-scielo-brasil) [admissão e a permanência de periódicos na Coleção SciELO Brasil](https://www.scielo.br/about/criterios-scielo-brasil) (versão mais atual) e Critérios de outras coleções da [Rede SciELO](https://www.scielo.org/).

Para fazer download deste documento sem perda de formatação, baixe no formato .pdf.

# **🔹VERSÃO** {#🔹versão}

A versão atual do [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema) é a 1.10 de junho de 2025\.

Duas versões são suportadas simultaneamente: a atual e a imediatamente anterior. Assim que lançada uma nova versão a versão anterior ainda é válida por **6 meses**.

Versões de correção tem um novo dígito adicionado ao final. Ex. 1.10.1. Versões de correção não quebram compatibilidade com sua versão original.

Versões anteriores:

* [SPS 1.9](https://scielo.readthedocs.io/projects/scielo-publishing-schema/pt_BR/1.9-branch/) \- março 2019 ***(suportada)***  
* [SPS 1.8.1](https://scielo.readthedocs.io/projects/scielo-publishing-schema/pt-br/1.8-branch/) \- maio 2019  
* [SPS 1.7](https://scielo.readthedocs.io/projects/scielo-publishing-schema/pt-br/1.7-branch/) \- setembro 2017  
* [SPS 1.6](http://docs.scielo.org/projects/scielo-publishing-schema/pt_BR/1.6-branch/). \- março 2017  
* [SPS 1.5.1](http://docs.scielo.org/projects/scielo-publishing-schema/pt_BR/1.5-branch/) \- setembro 2016  
* [SPS 1.4](http://docs.scielo.org/projects/scielo-publishing-schema/pt_BR/1.4-branch/) \- março 2016  
* [SPS 1.3](http://docs.scielo.org/projects/scielo-publishing-schema/pt_BR/1.3-branch/) \- setembro 2015  
* [SPS 1.2.1](http://docs.scielo.org/projects/scielo-publishing-schema/pt_BR/1.2-branch/) \- abril 2015  
* [SPS 1.1.1](http://docs.scielo.org/projects/scielo-publishing-schema/pt_BR/1.1-branch/) \- setembro 2014  
* [SPS 1.0](http://docs.scielo.org/projects/scielo-publishing-schema/pt_BR/1.0-branch/). \- janeiro 2014

# **🔹CONVENÇÕES UTILIZADAS NESTE GUIA** {#🔹convenções-utilizadas-neste-guia}

* Tags XML são indicadas no texto sempre entre brackets:

\<tag\>

* Atributos e valores de tags são identificados no texto em verde ou em formato de lista numerada e quando atributo será precedido de @:


valor  
@atributo="valor"

ou

1. @atributo1  
2. @atributo2

* Atributos e valores de atributos, quando indicados em tabela, terão cabeçalho e linhas verde:


| Valor | Descrição |
| :---: | :---: |
| valor | texto |


* Informações de onde os elementos aparecem e quantas vezes podem ocorrer, são indicados em tabela com cabeçalho cinza. 


| Aparece em | Ocorre |
| :---: | :---: |
| \<tag\> | vezes |


* Informações diversas, quando descritas em tabela, são indicadas com cabeçalho azul. 


| Texto | Texto |
| :---: | :---: |
| texto | texto |


* Exemplos de marcação XML são informados em caixa de código com texto verde:


```
<exemplo>
```


* Marcações que tenham especificidades relacionadas aos [Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf), terão o ícone da bandeira do Brasil com o texto: Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil. Ao clicar na bandeira pode-se acessar a página dos Critérios e na caixa de “Consulte” (azul) também será mencionado o link dos Critérios e a(s) seção(ões) em que é(são) mencionada(s) a(s) exigência(s).


[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil) Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

* Seções que tenham boas práticas de acessibilidade terão a descrição do símbolo de acessibilidade (versão ONU, 2015\) com o texto: Esta seção possui boas práticas de acessibilidade;  
* A estrela azul identifica o trecho textual explicando a regra para a acessibilidade.  
  * *Observação:* O [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema) possui uma seção com marcação XML exclusiva para a acessibilidade denominada [Marcação para Acessibilidade](#🔹marcação-para-acessibilidade), no entanto, outros elementos podem possuir indicação de boas práticas para acessibilidade.

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

* Caixas de Notas  
    
  * Caixa azul: Consulta para links externos ([Critérios SciELO Brasil](https://www.scielo.br/about/criterios-scielo-brasil), guias, sites, etc. \- exceto links [JATS 1.3](https://jats.nlm.nih.gov/publishing/tag-library/1.3/)).

| Consulte:  \[texto clicável com links externos ao SPS, exceto links JATS\] |
| :---- |

    

  * Caixa lilás: A seção comenta elementos, atributos e valores [JATS 1.3](https://jats.nlm.nih.gov/publishing/tag-library/1.3/).

| Consulte na JATS:  \[texto clicável com links JATS\] |
| :---- |

  * Caixa verde: A seção comenta outras seções do [SPS 1.10](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema).

| Consulte no SPS:  \[texto clicável com links SPS\] |
| :---- |

    

  * Caixa vermelha: Notas de atenção \- pode ou não mencionar links.

| Atenção:  \[texto\] |
| :---- |

# **🔹SUPORTE** {#🔹suporte}

Dúvidas e/ou comentários acerca da especificação [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema), deste guia de uso, das ferramentas disponibilizadas pela SciELO como apoio à marcação em XML, dos guias públicos e publicação de documentos, devem ser tratadas exclusivamente por meio de lista de discussão [scielo-xml](https://groups.google.com/g/scielo-xml?pli=1).

Como se inscrever

* **Envie um email para:** [scielo-xml+subscribe@googlegroups.com](mailto:scielo-xml+subscribe@googlegroups.com)

Como cancelar a inscrição

* **Envie um email para:** [scielo-xml+unsubscribe@googlegroups.com](mailto:scielo-xml+unsubscribe@googlegroups.com)

Acesse a lista através do link: [Google Groups: SciELO XML \- SciELO Publishing Schema](http://groups.google.com/group/scielo-xml).  

**Antes de perguntar na lista:**

* O título do email deve ser apresentado de forma clara e objetiva. Não use: Ajuda, Socorro, Dúvida, Consulta, etc.  
* Sempre confira o histórico da lista. É possível que algum participante tenha tido problemas iguais ou semelhantes e a resposta já foi postada; Lembre-se: o histórico da lista de discussão serve também como uma base de dados. Se você achou um post similar, mas ainda não entendeu o problema, preferencialmente responda em cima no post ou envie o email com o link do post e informe detalhadamente o que não ficou claro e sobre o que gostaria de obter mais informações. (Ver mais informações em: [Como fazer pesquisa de postagens na lista e como usar os marcadores](https://groups.google.com/g/scielo-xml/c/eOcYbSEgZgc))  
* Seu post deve ter documentos suficientes para que seja iniciado o suporte. Informar sobre um problema sem enviar o arquivo onde ocorre o problema e/ou o print do erro, invalida sua solicitação de suporte. Envie anexo no email o arquivo com problema (.doc, .docx, .xml, .pdf, imagens, etc.) e um print do erro. Envie o máximo de detalhes que puder. (Ver mais informações em: [Porque sua postagem não foi aprovada](https://groups.google.com/g/scielo-xml/c/0fqlH2vAn-8))  
* Verifique se seu post fere o código de conduta do SciELO Brasil. Nosso código de conduta é pautado pelos Princípios DEIA (Diversidade, Equidade, Inclusão e Acessibilidade), a fim de promovermos um espaço aberto, saudável e seguro a todos. Nós esperamos que todas as pessoas, enquanto estiverem participando de espaços promovidos pelo SciELO, sejam respeitosas e inclusivas, não façam ameaças de violência, não utilizem linguagem nociva, prejudicial ou preconceituosa (capacitista, racista, xenofóbica, LGBTFóbica, sexista, etarista, etc.), não façam ataques pessoais ou engajem em qualquer tipo de comportamento de assédio. Este tipo de comportamento não será tolerado e participantes que não aderirem ao código de conduta estão sujeitos a serem retirados da lista de discussão.   
* Certifique-se de que não há nenhuma documentação SciELO ou JATS que apresente respostas para suas dúvidas. Consulte antes:  
  * [Welcome to SciELO PC Programs’s documentation\!](https://docs.scielo.org/projects/scielo-pc-programs/en/latest/);  
  * [Vídeos de marcação](https://www.youtube.com/watch?v=L2Lzy_Icn88&list=PLQZT93bz3H79NTc-aUFMU_UZgo4Vl2iUH&index=2) (markup);  
  * SPS 1.10: [Guia para o uso de elementos e atributos XML em documentos que seguem a implementação SciELO Publishing Schema e outras informações relevantes à publicação](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema);  
  * SPS 1.19: [Guia de uso de elementos e atributos XML para documentos que seguem a implementação SciELO Publishing Schema](https://scielo.readthedocs.io/projects/scielo-publishing-schema/pt_BR/latest/);  
  * Journal Publishing Tag Library: [1.1](https://jats.nlm.nih.gov/publishing/tag-library/1.1/), [1.2](https://jats.nlm.nih.gov/publishing/tag-library/1.2/index.html) e [1.3](https://jats.nlm.nih.gov/publishing/tag-library/1.3/);  
  * [JATS4R](https://jats4r.org/);  
  * [Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil](https://www.scielo.br/about/criterios-scielo-brasil);  
  * [Guias SciELO](https://www.scielo.org/pt/sobre-o-scielo/metodologias-e-tecnologias/);  
  * [Parcerias](https://www.scielo.org/pt/sobre-o-scielo/parcerias/).

# **🔹DOCUMENTOS INDEXÁVEIS E NÃO INDEXÁVEIS**  {#🔹documentos-indexáveis-e-não-indexáveis}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Em SciELO Brasil, somente serão indexados documentos que apresentem conteúdo científico relevante e passível de estruturação em XML, segundo o SciELO Publishing Schema, que assegura a geração de metadados exaustivos para a indexação bibliográfica e bibliométrica.

Os seguintes tipos de documentos serão indexados, publicados e incluídos nas métricas de desempenho pelo SciELO: adendo, artigo de pesquisa, artigo de revisão, artigo de dados, carta, comentário de artigo, comunicação breve, comunicação rápida, diretrizes ou normas, discurso, discussão, editorial ou introdução, ensaio, entrevista, errata, métodos, obituário ou registro, parecer de artigo aprovado, posicionamento ou pensamento coletivo, relato de caso, resenha crítica de livro, resposta, retratação, retratação parcial e “outro” (quando o documento tem conteúdo científico que justifica sua indexação, mas nenhum dos tipos anteriores se aplica).

Editoriais de um número ou de introdução a uma seção são opcionais, mas devem tratar de temática científica passível de citação. Não são aceitáveis editoriais com simples relação dos artigos publicados ou de notícia relacionada com o periódico ou sua área temática, textos que atualmente são mais bem veiculados em blogs ou seções de notícias do website do periódico ou de sua instituição. Da mesma forma, somente serão aceitas resenhas de caráter crítico que aportam novos conhecimentos além do simples resumo de uma obra, obituários com análise da obra e da contribuição do autor homenageado com aporte de conteúdo científico e cartas sobre um tópico relevante ou de comentário a outros artigos. 

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.3. Tipos de documentos*. |
| :---- |

| Consulte no SPS:  [\<article\>: Artigo](#\<article\>:-artigo); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

## **Documentos Indexáveis** {#documentos-indexáveis}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

**Tabela A:** Documentos Indexáveis SciELO Brasil

| Tipos de documentos | Descrição do tipo de documento |
| ----- | ----- |
| **adendo** | Um trabalho publicado que agrega informação ou esclarecimento a outro trabalho (é diferente do tipo "errata" que corrige um erro em um material publicado previamente). |
| **artigo de pesquisa** | Artigo que comunica uma pesquisa original. |
| **artigo de dados** | Artigo que descreve dados de pesquisa no texto do artigo ou disponibilizados em um repositório de dados. |
| **artigo de revisão** | Artigo que sumariza criticamente o conhecimento científico sobre um determinado tema. Também conhecido como revisão de literatura. |
| **carta** | Carta dirigida ao periódico, tipicamente comentando um trabalho publicado. |
|  **comentário de artigo** | Um documento cujo objeto ou foco é(são) outro(s) documento(s); documento que comenta outros documentos. Este tipo de documento pode ser usado quando o(a) editor(a) de uma publicação convida um(a) autor(a) com uma opinião oposta para comentar um documento controverso e então publica os dois documentos juntos. O tipo "editorial" que tem similaridade é reservado para comentários escritos pelo(a) editor(a) ou membro da equipe editorial ou autor(a) convidado(a). |
| **comunicação breve** | Comunicação sucinta de resultados de pesquisa. |
| **comunicação rápida** | Atualização de uma pesquisa ou outros itens noticiosos. |
| **diretrizes ou normas** | Documento de um guia ou diretriz estabelecida por uma autoridade biomédica ou de outra área como um comitê, sociedade, ou agência do governo. |
| **discurso** | Documento de uma fala ou apresentação oral. |
| **discussão** | Discussão convidada relacionado com um documento específico ou um número do periódico. |
| **editorial ou introdução** | Peça de opinião, declaração política ou comentário geral escrito por membro da equipe editorial (com autoria e título próprio diferente do título da seção). |
| **ensaio** | Reflexão circunstanciada, com maior liberdade por parte do(a) autor(a) para defender determinada posição, que vise a aprofundar a discussão ou que apresente nova contribuição/abordagem a respeito de tema relevante. |
| **entrevista** | Ato de entrevistar ou ser entrevistado(a). É uma conversa entre duas ou mais pessoas com um fim determinado com perguntas feitas pelo(a) entrevistador(a) de modo a obter informação necessária por parte do(a) entrevistado(a). |
| **errata** | Modificação ou correção de material publicado previamente. Em inglês é chamado também de "*correction*". (O tipo "adendo" aplica-se apenas para material adicionado a um material publicado previamente). |
| **Expediente Anual (*Ad Hoc*)** | Documento de publicação anual que apresenta a composição completa do corpo editorial, incluindo editores de seção e conselho editorial, bem como a lista de pareceristas ad hoc que colaboraram com o processo de revisão por pares ao longo do ano. Este documento visa a registrar e agradecer a contribuição de todos os envolvidos na publicação do periódico, reforçando o compromisso com a transparência editorial. |
| **Manifestação de Preocupação** | Documento publicado pelo periódico para alertar os leitores sobre possíveis problemas em um documento previamente publicado, como indícios de má conduta ou erros graves ainda em investigação. A manifestação não confirma as suspeitas, mas informa a comunidade científica até a conclusão da apuração. |
|  **métodos** | Documento que descreve avanços metodológicos, incluindo métodos inovadores e aprimoramento de métodos existentes. O documento deve incluir evidências da eficácia do método e comparações com os métodos anteriormente disponíveis. |
| **obituário, registro** | Anúncio do falecimento ou elogio a um(a) colega falecido(a) recentemente, com análise da obra e da contribuição do autor homenageado com aporte de conteúdo científico. |
| **parecer de artigo aprovado** | Documento de análise de um manuscrito que comunica pesquisa com avaliação da sua relevância, dos métodos aplicados e apresentação e discussão dos resultados obtidos. O parecer destaca as contribuições da pesquisa que recomendam sua aceitação e as recomendações de correções e aperfeiçoamentos. |
| **posicionamento ou pensamento coletivo** | Documento, posição ou pensamento coletivo elaborado em conjunto com pesquisadores(as) experts em determinados assuntos. |
| **relato de caso** | Estudo de caso, relato de caso, ou outra descrição de um caso. |
| **resenha crítica de livro** | Resenha ou análise crítica de um ou mais livros impressos ou online. (O tipo "revisão de produto" é usado para análise de produtos). |
| **resposta** | Resposta a uma carta ou comentário, tipicamente pelo(a) autor(a) original comentando sobre comentários. |
| **retratação** | Retratação ou negação de um material publicado previamente. |
| **retratação parcial** | Retratação ou negação de parte ou partes de material publicado previamente. |
| **outro** | Quando o documento tem conteúdo científico que justifica sua indexação mas nenhum dos tipos anteriores se aplica. |

***Fonte:*** [Critérios SciELO Brasil](https://www.scielo.br/about/criterios-scielo-brasil), setembro 2024\.

## **Documentos Não Indexáveis** {#documentos-não-indexáveis}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

**Tabela B:** Documentos não Indexáveis SciELO Brasil

| Tipos de documentos | Descrição do tipo de documento |
| ----- | ----- |
| **anais** | Material publicado em congresso. |
| **anúncios** | Material anunciado no periódico (pode ou não estar diretamente relacionado com o periódico). |
| **calendário** | Lista de eventos. |
| **chamadas** | Sumário ou chamada de itens do número corrente do periódico. |
| **livros recebidos** | Notificação que itens, como livros ou outros trabalhos, foram recebidos pelo periódico para análise ou consideração. |
| **notícias** | Notícias, normalmente atuais, mas, atipicamente, históricas. |
| **reimpressões** | Reimpressões de documentos publicados previamente. |
| **relatórios de reunião** | Relatórios de conferências, simpósios ou reuniões. |
| **Resumos, resumos expandidos ou resumos de teses** | Os documentos propriamente ditos são resumos (de *papers* ou apresentações) que normalmente foram apresentados ou publicados separadamente. |
| **revisões de produtos** | Descrições, análises ou revisões de produtos ou serviços, como por exemplo, um pacote de software (O tipo "resenha de livro" é usado para a análise de livros). |
| **teses** | Teses ou dissertações escritas como parte da finalização de cursos. |
|  **Traduções** | Traduções de documentos escritos em outros idiomas e já publicados **Nota:** Os artigos podem ser inéditos ou disponibilizados previamente em servidores de preprints reconhecidos pelo periódico. Não se permite a duplicação de publicação ou tradução de artigo já publicado em outro periódico ou como capítulo de livro. Entretanto, são aceitáveis documentos derivados de documentos originais segundo as regras das licenças Creative Commons ou outras licenças e que se caracterizam como um novo documento com autoria e DOI próprio. |

***Fonte:*** [Critérios SciELO Brasil](https://www.scielo.br/about/criterios-scielo-brasil), setembro 2024\.

## **Equivalência entre documentos indexáveis e @article-type em \<article\>** {#equivalência-entre-documentos-indexáveis-e-@article-type-em-<article>}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Na marcação XML os documentos devem ter sua representação tipológica através da tag [\<article\>](#\<article\>:-artigo) e seu atributo @article-type.

**Tabela C:** Equivalência de tipos de documentos [SPS](#\<article\>:-artigo) x [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf) 

| @article-type | Tipos de documentos | Descrição do tipo de documento |
| :---: | ----- | ----- |
| addendum | **adendo** | Um trabalho publicado que agrega informação ou esclarecimento a outro trabalho (é diferente do tipo "errata" que corrige um erro em um material publicado previamente). |
| research-article | **artigo de pesquisa** | Artigo que comunica uma pesquisa original. |
| data-article | **artigo de dados** | Artigo que descreve dados de pesquisa no texto do artigo ou disponibilizados em um repositório de dados. |
| review-article | **artigo de revisão** | Artigo que sumariza criticamente o conhecimento científico sobre um determinado tema. Também conhecido como revisão de literatura. |
| letter | **carta** | Carta dirigida ao periódico, tipicamente comentando um trabalho publicado. |
|  article-commentary |  **comentário de artigo** | Um documento cujo objeto ou foco é(são) outro(s) documentos; documento que comenta outros documentos. Este tipo de documento pode ser usado quando o(a) editor(a) de uma publicação convida um(a) autor(a) com uma opinião oposta para comentar um documento controverso e então publica os dois documentos juntos. O tipo "editorial" que tem similaridade é reservado para comentários escritos pelo(a) editor(a) ou membro da equipe editorial ou autor(a) convidado(a). |
| brief-report  | **comunicação breve** | Comunicação sucinta de resultados de pesquisa. |
| rapid-communication | **comunicação rápida** | Atualização de uma pesquisa ou outros itens noticiosos. |
| clinical-instruction | **diretrizes ou normas** | Documento de um guia ou diretriz estabelecida por uma autoridade biomédica ou de outra área como um comitê, sociedade, ou agência do governo. |
| oration | **discurso** | Documento de uma fala ou apresentação oral. |
| discussion | **discussão** | Discussão convidada relacionado com um documento específico ou um número do periódico. |
| editorial | **editorial ou introdução** | Peça de opinião, declaração política ou comentário geral escrito por membro da equipe editorial (com autoria e título próprio diferente do título da seção). |
| review-article | **ensaio** | Reflexão circunstanciada, com maior liberdade por parte do(a) autor(a) para defender determinada posição, que vise a aprofundar a discussão ou que apresente nova contribuição/abordagem a respeito de tema relevante. |
| other  | **entrevista** | Ato de entrevistar ou ser entrevistado(a). É uma conversa entre duas ou mais pessoas com um fim determinado com perguntas feitas pelo(a) entrevistador(a) de modo a obter informação necessária por parte do(a) entrevistado(a). |
| correction | **errata** | Modificação ou correção de material publicado previamente. Em inglês é chamado também de "*correction*". (O tipo "adendo" aplica-se apenas para material adicionado a um material publicado previamente). |
| other | **Expediente Anual (*Ad Hoc*)** | Documento de publicação anual que apresenta a composição completa do corpo editorial, incluindo editores de seção e conselho editorial, bem como a lista de pareceristas ad hoc que colaboraram com o processo de revisão por pares ao longo do ano. Este documento visa a registrar e agradecer a contribuição de todos os envolvidos na publicação do periódico, reforçando o compromisso com a transparência editorial. |
| expression-of-concern | **Manifestação de Preocupação** | Documento publicado pelo periódico para alertar os leitores sobre possíveis problemas em um documento previamente publicado, como indícios de má conduta ou erros graves ainda em investigação. A manifestação não confirma as suspeitas, mas informa a comunidade científica até a conclusão da apuração. |
| review-article | **métodos** | Documento que descreve avanços metodológicos, incluindo métodos inovadores e aprimoramento de métodos existentes. O documento deve incluir evidências da eficácia do método e comparações com os métodos anteriormente disponíveis. |
| obituary | **obituário, registro** | Anúncio do falecimento ou elogio a um(a) colega falecido(a) recentemente, com análise da obra e da contribuição do autor homenageado com aporte de conteúdo científico. |
| reviewer-report | **parecer de artigo aprovado** | Documento de análise de um manuscrito que comunica pesquisa com avaliação da sua relevância, dos métodos aplicados e apresentação e discussão dos resultados obtidos. O parecer destaca as contribuições da pesquisa que recomendam sua aceitação e as recomendações de correções e aperfeiçoamentos. |
| article-commentary | **posicionamento ou pensamento coletivo** | Documento, posição ou pensamento coletivo elaborado em conjunto com pesquisadores(as) experts em determinados assuntos. |
| case-report | **relato de caso** | Estudo de caso, relato de caso, ou outra descrição de um caso. |
| book-review | **resenha crítica de livro** | Resenha ou análise crítica de um ou mais livros impressos ou online. |
| reply | **resposta** | Resposta a uma carta ou comentário, tipicamente pelo(a) autor(a)original comentando sobre comentários. |
| retraction | **retratação** | Retratação ou negação de um material publicado previamente. |
| partial-retraction | **retratação parcial** | Retratação ou negação de parte ou partes de material publicado previamente. |
| other | **outro** | Quando o documento tem conteúdo científico que justifica sua indexação mas nenhum dos tipos anteriores se aplica. |

| Consulte no SPS:   @article-type em [\<article\>: Artigo](#\<article\>:-artigo);  @article-type em [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

# **🔹ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS** {#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Segundo os [Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf) para publicação de qualquer tipo de documento indexável na coleção [SciELO Brasil](https://www.scielo.br/), é necessário a presença obrigatória de **6 elementos**:

1. Seção no mesmo idioma do documento;  
2. Título do documento diferente do título da seção;  
3. Autoria com ORCID;  
4. Afiliação institucional dos(as) autores(as);  
5. Uma ou mais citações no texto;  
6. Lista de referências bibliográficas das citações no corpo do texto.

   

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*:* *5.2.8.1. Textos em XML – SciELO Publishing Schema* e *5.2.8.3. Identificação ORCID iD.*   |
| :---- |

| Consulte no SPS:  [\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto); [\<contrib-group\>: Autoria](#\<contrib-group\>:-autoria); [\<contrib-id\>: ORCID](#\<contrib-id\>:-orcid); [\<aff\>: Afiliação de Autores](#\<aff\>:-afiliação-de-autores); [\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada); [\<ref-list\>: Lista de Referências](#\<ref-list\>:-lista-de-referências). |
| :---- |

   

## **Excepcionalidade para os 6 elementos obrigatórios para publicação** {#excepcionalidade-para-os-6-elementos-obrigatórios-para-publicação}

Alguns tipos de documentos poderão ser publicados na coleção SciELO Brasil sem alguns dos elementos obrigatórios para publicação. Na tabela a seguir o “x” mostra quais são os dados obrigatórios para cada um deles.

| Documentos | Seção | Título | Autoria | Afiliação | Citação | Referência |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Errata** | x | x |  |  |  |  |
| **Retratação** | x | x |  |  |  |  |
| **Adendo** | x | x |  |  |  |  |
| **Manifestação de Preocupação** | x | x |  |  |  |  |
| **Parecer** | x | x | x |  |  |  |
| **Ad Hoc** | x | x |  |  |  |  |

| Atenção:  *Ad Hoc* podem ser publicados apenas uma vez ao ano; A autoria do parecer pode ser anônima \<anonymous/\>. |
| :---- |

| Consulte:  [Guia para Publicação de Errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf); [Guia para Publicação de Retratação](https://wp.scielo.org/wp-content/uploads/guia_retratacao.pdf); [Guia para Publicação de Adendo](https://wp.scielo.org/wp-content/uploads/guia_adendo.pdf); [Guia para publicação de Manifestação de Preocupação](https://wp.scielo.org/wp-content/uploads/guia_manifestacao.pdf). |
| :---- |

| Consulte no SPS:  [Equivalência entre documentos indexáveis e @article-type em \<article\>](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>); [Adendo](#adendo); [Errata](#errata); [Retratação](#retratação); [Manifestação de Preocupação](#manifestação-de-preocupação); [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta). |
| :---- |

## **Excepcionalidade para documentos Retrospectivos** {#excepcionalidade-para-documentos-retrospectivos}

Documentos retrospectivos são aqueles publicados em volumes/números de dois anos anteriores ao ano corrente.

Os pacotes retrospectivos devem ser enviados para publicação em formato XML, com o texto completo devidamente marcado da mesma forma que os pacotes de atualização, e seguindo as orientações da seção [ENTREGA DE PACOTE XML PARA PUBLICAÇÃO](#🔹entrega-de-pacote-xml-para-publicação).

É importante destacar que a prioridade de publicação é dada aos pacotes de atualização. Portanto, a disponibilização dos documentos retrospectivos será realizada gradualmente, de acordo com o processamento desses documentos, desde que não comprometa o processo regular de atualização da coleção.

As exigências dos [Critérios SciELO Brasil](https://www.scielo.br/about/criterios-scielo-brasil) vigente podem variar conforme o ano do volume/número ao qual pertence o documento. A tabela a seguir apresenta as exigências por ano e informa quais se aplicam ao PDF e/ou ao XML.

Independentemente do ano, **título, seção e autoria do documento são obrigatórios**. Além disso, documentos indexáveis e não indexáveis devem sempre seguir as diretrizes estabelecidas pelos Critérios SciELO e pelo SPS vigentes.

| Exigências  | Exigido a partir de (Ano) | Onde se aplicam (PDF / XML) |
| :---: | :---: | :---: |
| Afiliação (instituição e dados geográficos) | 2010 e anos posteriores | XML e PDF |
| Tabelas e fórmulas codificadas | 2017 e anos posteriores | XML |
| Datas de histórico de recebimento e aprovação (com data completa: dia, mês e ano) | 2017 e anos posteriores | XML e PDF |
| Licença Creative Commons | 2018 e anos anteriores | No XML pelo menos (2019 e anos posteriores obrigatório no XML e PDF) |
| DOI  | 2018 e anos anteriores | No XML pelo menos (2019 e anos posteriores obrigatório no XML e PDF) |
| ORCID | 2020 e anos posteriores | XML e PDF |
| DOI diferente do documento original para tradução | 2022 e anos posteriores | XML e PDF |
| Uma ou mais citações no texto | 2022 e anos posteriores | XML e PDF |
| Lista de referências bibliográficas das citações no corpo do texto  | 2022 e anos posteriores | XML e PDF |
| Licença Creative Commons do tipo BY | 2024 e anos posteriores | XML e PDF |
| Publicação Contínua (PC) | 2024 e anos posteriores | XML e PDF |
| Idioma da seção no mesmo idioma do texto | 2024 e anos posteriores | XML e PDF |
| Declaração de Disponibilidade de Dados | 2025 e anos posteriores | XML e PDF |
| Declaração de Editor Responsável pelo Processo de Avaliação | 2025 e anos posteriores | XML e PDF |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/about/criterios-scielo-brasil); [Comunicado](http://us4.campaign-archive2.com/?u=f26dcf71797dd37381acb4aa5&id=0211ed957f&e=%5BUNIQID) codificação de fórmulas e tabelas enviado em 09/12/2016; [Comunicado](https://mailchi.mp/scielo/doi-traducoes) DOIs para tradução enviado em 23/03/2022; [Comunicado](https://us4.campaign-archive.com/?u=f26dcf71797dd37381acb4aa5&id=2a6634a845) sobre a obrigatoriedade de datas completas, enviado em 21/10/2016; [Guia para Implementação da Modalidade de Publicação Contínua em Periódicos Científicos](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf); [Orientação para criação do DOI](https://wp.scielo.org/wp-content/uploads/orientacao_doi.pdf); [Diretrizes para exibição de DOIs do Crossref](https://wp.scielo.org/wp-content/uploads/Diretriz_DOI_PT.pdf); [Guia do usuário do Digital Object Identifier](https://www.abecbrasil.org.br/arquivos/Guia_usuario_DOI-online3.pdf). |
| :---- |

| Consulte no SPS:  [ENTREGA DE PACOTE XML PARA PUBLICAÇÃO](#🔹entrega-de-pacote-xml-para-publicação); [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados); [Declaração de Editor Responsável pelo Processo de Avaliação](#declaração-de-editor-responsável-pelo-processo-de-avaliação). |
| :---- |

# **🔹MODALIDADES DE PUBLICAÇÃO ACEITAS** {#🔹modalidades-de-publicação-aceitas}

Há duas modalidades de publicação aceitas para a publicação nas coleções:

1. **Modalidade de [Publicação Contínua (PC)](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf):**  
   1. Publicação de artigos em lotes sem a necessidade de esperar a composição completa dos fascículos ou de edições seriadas;  
   2. Permite a publicação simultânea de artigos em vários números aberto ou em um volume anual;  
2. **Modalidade de Regular:**  
   1. Publicação de artigos reunidos e disponibilizados somente após a composição completa de um fascículo ou edição seriada;  
   2. Não permite a inclusão de artigos posteriores à publicação do número.  
      

| Atenção:  Para a coleção SciELO Brasil a modalidade de [Publicação Contínua (PC)](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf) é mandatória a partir de 2025; A modalidade de publicação Ahead of Print (AOP) não é aceita. |
| :---- |

| Consulte:  [Guia para Implementação da Modalidade de Publicação Contínua em Periódicos Científicos](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf). |
| :---- |

## **Características da Publicação Regular** {#características-da-publicação-regular}

Periódicos que adotam modalidade regular atendem as características a seguir:

1. Publicação impressa e digital;  
2. Paginação sequencial de um número, seguindo a paginação do próximo artigo onde o anterior parou;  
3. Ordenação de documentos através paginação sequencial;  
4. Seções usualmente indicadas pelo sumário impresso (podem ou não existir no documento).

## **Características da Publicação Contínua** {#características-da-publicação-contínua}

Periódicos que adotam modalidade de publicação contínua obrigatoriamente atendem as características a seguir:

1. Publicação puramente digital (se necessário realizam impressão sob demanda);  
2. Paginação digital usando o elocation-id;  
3. Paginação sequencial apenas como paginação de impressão sempre iniciada em 1 a cada documento (1-12, 2-12, 3-12, etc.);  
4. Indicação de seção no documento;  
5. Ordenação de documentos através de número [other](#\<article-id\>:-doi-e-other)*.*

| Consulte:  [Guia para Implementação da Modalidade de Publicação Contínua em Periódicos Científicos](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf); [Guia para a Gestão e Criação de Números Other para Ordenação de Seções de Periódicos que Adotam a Modalidade de Publicação Contínua (PC)](https://docs.google.com/document/d/1HvLyoMxBF0WWTbwnDLYGbQmf9Z6OkrL-x8EWaGmRnbg/edit?tab=t.0) |
| :---- |

| Consulte no SPS:  [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other). |
| :---- |

   

### **Antecipação e Restrições de Publicação na Modalidade Contínua** {#antecipação-e-restrições-de-publicação-na-modalidade-contínua}

Recomenda-se para todas as coleções que os periódicos atendam aos requisitos a seguir, sendo exigência obrigatória para periódicos indexados na coleção SciELO Brasil.

- Publicar dentro do ano corrente com **antecipação** de no máximo **6 a 8 meses**.   
- Os periódicos podem, a partir de **novembro do ano corrente**, abrir o volume e/ou números do próximo ano.   
  - Para SciELO Brasil, antes de novembro lotes entregues para volume e/ou números do ano seguinte não serão aceitos para a publicação;  
  - Caso seja de interesse do periódico exclusivamente indexado na coleção SciELO Brasil, recomenda-se que os artigos já aprovados sejam disponibilizados em SciELO Preprints, não sendo necessária a marcação em XML. Para orientações sobre o depósito dos artigos, a equipe editorial do periódico indexado na coleção SciELO Brasil deve entrar em  contato com a equipe do [SciELO Preprints](https://preprints.scielo.org/index.php/scielo) através do e-mail \<[scielo.submission@scielo.org](mailto:scielo.submission@scielo.org)\>. Consulte também a [FAQ](https://preprints.scielo.org/index.php/scielo/faq) do SciELO Preprints.  
    

| Atenção:  É importante que os periódicos levem em consideração que, mesmo adiantando a publicação para o ano seguinte, a partir de novembro, as métricas de citação, tais como: Journal Impact Factor (WoS), SCImago Journal Rank (SCImago) e CiteScore (Scopus), não vão considerar citações para artigos publicados com data futura fora do ano corrente. Estes índices consideram as citações durante o ano corrente, para artigos publicados datados de um determinado período dos anos anteriores, ou citações do mesmo ano, para artigos publicados datados do mesmo ano. A publicação adiantada é considerada apenas para o índice de imediatez, portanto, recomenda-se que os periódicos usem a modalidade de publicação contínua para publicar o mais rápido possível sem atrasos, mas, considerando o ano real de sua publicação. |
| :---- |

Outro ponto a considerar é que não é indicada a “publicação no passado”, ou seja, os periódicos não devem utilizar a modalidade contínua, que tem o intuito de adiantar a publicação, de forma contrária. O uso desta modalidade de forma incorreta acarreta as seguintes consequências: 

* Os artigos estarão associados ao ano anterior, mas sua efetiva data de publicação eletrônica será o ano vigente, evidenciando atraso na publicação;   
* Os artigos perdem um ano de exposição, porque mesmo estando em um volume/número do ano anterior, foram efetivamente publicados somente no ano vigente. 

Caso o periódico não consiga publicar todos os artigos do ano corrente (e, portanto, que tenha encerrado os números propostos dentro da periodicidade adotada), até o encerramento do ano, os artigos ainda em processo deverão ser transferidos para o ano seguinte, quando serão efetivamente publicados no volume e/ou números do ano corrente.

# **🔹ENTREGA DE PACOTE XML PARA PUBLICAÇÃO** {#🔹entrega-de-pacote-xml-para-publicação}

Esta seção descreve os procedimentos para a entrega de pacotes XML à unidade de produção para publicação nas coleções:

* [SciELO Brasil](https://www.scielo.br/);  
* [SciELO Saúde Pública](https://www.scielosp.org/);  
* [RevEnf](https://www.revenf.bvs.br/);  
* [Pepsic](https://pepsic.bvsalud.org/).

As entregas são realizadas exclusivamente por [Empresas com atestado de capacidade técnica para serviços de marcação de textos de acordo com SciELO Publishing Schema (SciELO PS)](https://www.scielo.org/pt/sobre-o-scielo/parcerias/empresas-com-atestado-de-capacidade-para-marcacao-de-textos/) contratadas pelos periódicos indexados nas coleções ou pelas equipes editoriais dos periódicos que desenvolveram a capacidade internamente. 

Nenhuma das coleções publica artigos enviados diretamente pelos autores. As coleções são compostas por artigos publicados em periódicos científicos, portanto os autores deverão submeter seu artigo para um dos periódicos que fazem parte da coleção e, caso aceito, posteriormente o artigo estará disponível na página da revista na coleção selecionada.

## **O Que é um Pacote de Entrega** {#o-que-é-um-pacote-de-entrega}

É considerado um pacote de entrega um .zip com um ou mais artigos de uma revista para um volume/número.

Um pacote de entrega para uma revista que adota a modalidade de publicação contínua (PC) é denominado “lote” e representa um artigo ou um conjunto de até **5 XMLs** que já estão prontos para publicação e que pertencem ao mesmo volume/número.

Já um pacote de entrega de uma revista que adota a modalidade regular representa o conjunto de artigos que compõem todo um volume/número.

## **O que Contém um Pacote de Entrega** {#o-que-contém-um-pacote-de-entrega}

O envio do pacote de entrega deve conter:

1. XML dos artigos:   
   1. Um único XML para cada documento, sendo:    
      1. Elemento \<article\> para identificar o documento no idioma original;  
      2. Elemento \<sub-article\> para identificar o(s) documento(s) de tradução. \- *se houver*  
   2. É importante destacar que traduções obrigatoriamente devem ser enviadas no mesmo XML no documento no idioma original. Não são aceitas traduções de documentos já publicados anteriormente, caracterizando dupla publicação.   
2. PDF dos artigos:  
   1. Um PDF para o documento no idioma original;  
   2. Um PDF para cada documento nos idiomas de tradução. \- *se houver*   
3. Ativos Digitais dos documentos: \- *se houver*  
   1. Imagem/Figura;  
   2. Material Suplementar;  
   3. Vídeo;   
   4. Áudio.  
4. Relatório XPM (Gerado pelo XML Package Maker)  
5. Sumário em PDF \- *apenas para revista publicando em modalidade regular* 

| Consulte:  Para mais informações sobre o XPM acessar o link [Package Maker \- Como usar?](http://docs.scielo.org/projects/scielo-pc-programs/en/latest/pt_how_to_validate_xml_package.html), para baixar a ferramenta acesse o link [Download](http://docs.scielo.org/projects/scielo-pc-programs/en/latest/download.html#download).   |
| :---- |

## **Formatos Permitidos para os Arquivos que Compõem o Pacote de Entrega** {#formatos-permitidos-para-os-arquivos-que-compõem-o-pacote-de-entrega}

Os formatos e extensões permitidas para os arquivos que compõem um pacote de entrega são:

| Arquivo | Formato / Extensão |
| ----- | ----- |
| **Pasta** | .zip (não usar .rar) |
| **XML** | .xml |
| **PDF** | .pdf |
|  **Imagem** | .jpg / .jpeg *(preferencialmente use este formato)* .png  .tif / .tiff  .svg *(apenas em [\<alternatives\>](#\<alternatives\>:-.svg))* |
| **Vídeo** | .mp4 |
| **Relatório XPM** | .html |
| **Sumário Impresso** | .pdf |

Tablas, equações e fórmulas que compõem o documento devem ser identificadas diretamente no XML seguindo a marcação:

| Arquivo | Formato / Extensão |
| ----- | ----- |
| **Tabela**  | [NISO JATS table model](https://jats.nlm.nih.gov/archiving/tag-library/1.3/element/table.html) e [Table Formatting](http://jats.nlm.nih.gov/publishing/tag-library/1.0/n-unw2.html#pub-tag-table-format) .svg *(apenas em [\<alternatives\>](#\<alternatives\>:-.svg))* |
|  **Fórmula e Equação**  | [MathML](http://www.w3.org/TR/MathML3/) LaTeX TeX .svg *(apenas em [\<alternatives\>](#\<alternatives\>:-.svg))* |

* Apêndices e Anexos devem seguir as regras descritas na seção: [\<app-group\>: Apêndice e Anexo](#\<app-group\>:-apêndice-e-anexo);  
* Materiais Suplementares devem seguir as regras descritas na seção: [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar);  
* Dados de pesquisa devem seguir as regras descritas na seção: [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados).

## **Acrônimos Oficiais dos Periódicos** {#acrônimos-oficiais-dos-periódicos}

O acrônimo de um periódico é uma sigla oficial definida pelas coordenações das coleções e que compõem alguns dados, como:

* Elemento XML [\<journal-id journal-id-type="publisher-id"\>](#\<journal-meta\>:-metadados-do-periódico);  
* Nomeação de arquivos;  
* Nomeação de Pastas.

Para acessar os títulos dos periódicos correntes com seus acrônimos correspondentes das coleções [SciELO Brasil](https://www.scielo.br/); [SciELO Saúde Pública](https://www.scielosp.org/); [RevEnf](https://www.revenf.bvs.br/) e [Pepsic](https://pepsic.bvsalud.org/), acesse a planilha oficial [Acrônimos](https://docs.google.com/spreadsheets/d/1Sg7PX9eZYfXbKqm7oVnMI-SZ-jxsBJIdwnVJzRPEP1s/edit?gid=0#gid=0).

## **Regras para Nomeação de Arquivos e Pastas** {#regras-para-nomeação-de-arquivos-e-pastas}

Os arquivos XML, PDF e ativos digitais (como figuras, anexos, material suplementar, etc.), que compõem os pacotes de entrega para publicação e suas pastas, devem possuir obrigatoriamente a estrutura de nomeação mencionada a seguir.

### **Nomeação de Arquivos** {#nomeação-de-arquivos}

Itens separados por hífen \- na nomeação:

- **ISSN:** Número do periódico registrado no ISSN (Se houver mais de um, dar preferência ao online);  
- **Acrônimo:** Sigla do periódico na Coleção (consulte planilha [Acrônimos](https://docs.google.com/spreadsheets/d/1Sg7PX9eZYfXbKqm7oVnMI-SZ-jxsBJIdwnVJzRPEP1s/edit?gid=0#gid=0));  
- **Volume/Número/Número Especial/Suplemento:** Indicação numérica, apenas de volume e número, e indicação de sigla mais número, para suplemento e números especial, usando as siglas:   
  * **nspe** \= número especial   
  * **s** \= suplemento  
- **Elocation-id:** Paginação online do documento (se documento na modalidade de [publicação contínua](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf));  
- **Paginação:** Informação da primeira página do documento (se documento na modalidade regular);  
- **Idioma:** Apenas para arquivos de tradução (arquivos do idioma original mantêm-se sem dados de idioma na nomeação). Para arquivos traduzidos, usar:  
  * **\-en** \= inglês  
  * **\-pt** \= português  
  * **\-es** \= espanhol

**Exemplos para nomeação de arquivos:**

- **ISSN fictício:** 0124-4567

- **Acrônimo fictício:** scie

* *Documento para o volume 10 \+ número 3* 

* *Exemplo: idioma original inglês*

* *Modalidade Regular*

  * *ISSN-Acrônimo-Volume-Número-Paginação Inicial*

  * **0124-4567-scie-10-3-365**

* *Documento para o volume 10 \+ número 3* 

* *Exemplo: Tradução do arquivo em português*

* *Modalidade Regular*

  * *ISSN-Acrônimo-Volume-Número-Paginação Inicial-Idioma*

  * **0124-4567-scie-10-3-365-pt**

* *Documento para o volume 9*

* *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Elocation*

  * **0124-4567-scie-9-e3652023**

* *Documento para o volume 56 \+ suplemento 2*

* *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Suplemento-Elocation*

  * **0124-4567-scie-56-s2-e689**

* *Documento para o volume 27 \+ número 4 \+ suplemento 1*

* *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Número-Suplemento-Elocation*

  * **0124-4567-scie-27-4-s1-e56**

* *Documento para o volume 37 \+ número especial 1*

* *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Número Especial-Elocation*

  * **0124-4567-scie-37-nspe1-e6398**


| Atenção:  Lotes para entrega de periódicos que adotam a modalidade de publicação contínua devem possuir até 5 arquivos XMLs; Números especiais e suplementos obrigatoriamente devem ter a indicação numérica 1 em diante nos PDFs e XMLs; Sempre usar hífen sem espaço para separação dos itens que compõem a nomeação dos arquivos; É proibido o uso de underline, ponto, en dash, em dash, acentuação, espaço ou caracteres especiais para nomeação de arquivos. |
| :---- |

### **Nomeação de Pastas** {#nomeação-de-pastas}

Itens separados por hífen \- na nomeação:

- **ISSN:** Número do periódico registrado no ISSN (Se houver mais de um, dar preferência ao online);  
- **Acrônimo:** Sigla do periódico na Coleção (consulte planilha [Acrônimos](https://docs.google.com/spreadsheets/d/1Sg7PX9eZYfXbKqm7oVnMI-SZ-jxsBJIdwnVJzRPEP1s/edit?gid=0#gid=0));  
- Indicação numérica, apenas de volume e número, e indicação de sigla mais número, para suplemento e números especial, usando as siglas:   
  * **nspe** \= número especial   
  * **s** \= suplemento  
- **Lote:** Para pastas de pacotes XML da modalidade de [publicação contínua](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf), adiciona-se dois dígitos da sequência do lote \+ dois últimos dígitos do ano **ao qual pertence o volume/número do(s) documento(s)**. O número do lote deve ser enviado pelos prestadores de serviço XML contratados pelos periódicos ou pelas equipes editoriais dos periódicos, quando estes produzirem seus XMLs, sempre na ordem sequencial, iniciando em 01 a cada volume, número, número especial e suplemento.

**Exemplos para nomeação de pastas:**

- **ISSN fictício:** 0124-4567

- **Acrônimo fictício:** scie

* *Pasta para o(s) documento(s) do volume 10 \+ número 3*

  * *Modalidade de Publicação Regular*

  * *ISSN-Acrônimo-Volume-Número*

  * **0124-4567-scie-10-3**

* *Pasta para o(s) documento(s) do Primeiro lote do volume 10 \+ número 3 de 2025*

  * *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Número-Lote*

  * **0124-4567-scie-10-3-0125**

* *Pasta para o(s) documento(s) do Terceiro lote do volume 23 \+ suplemento 1 de 2025*

  * *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Suplemento-Lote*

  * **0124-4567-scie-23-s1-0325**

* *Pasta para o(s) documento(s) do Sexto lote do volume 46 \+ número 2 \+ suplemento 1 de 2024*

  * *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Número-Suplemento-Lote*

  * **0124-4567-scie-v46-2-s1-0624**

* *Pasta para o(s) documento(s) do Décimo Terceito lote volume 123 \+ número especial 1 de 2026*

  * *Modalidade de Publicação Contínua*

  * *ISSN-Acrônimo-Volume-Número Especial-Lote*

  * **0124-4567-scie-123-nspe1-1326**


| Atenção:  Lotes para entrega de periódicos que adotam a modalidade de publicação contínua devem possuir até 5 arquivos XMLs; A indicação de lote não tem relação com o ano de entrega e sim com o ano do volume e/ou número ao qual o(s) documento(s) pertence(m); Números especiais e suplementos obrigatoriamente devem ter a indicação numérica 1 em diante nos PDFs e XML; Sempre usar hífen sem espaço para separação dos itens que compõem a nomeação de pastas; É proibido o uso de underline, ponto, en dash, em dash, acentuação, espaço ou caracteres especiais para nomeação de pastas. |
| :---- |

# 

**Estrutura da Pasta .zip do Pacote de Entrega**

Os arquivos que compõem o pacote devem estar na seguinte estrutura para depósito no FTP:

* pasta.zip  
  * pasta  
    * relatório xpm  
    * arquivo1.pdf  
    * arquivo1.xml  
    * arquivo1.jpg  
    * arquivo2.pdf  
    * arquivo2.xml  
    * arquivo2.jpg  
    * …

Exemplo:

* 0124-4567-scie-10-3-0125.zip  
  * 0124-4567-scie-10-3-0125  
    * xpm.html  
    * 0124-4567-scie-10-3-e333.pdf  
    * 0124-4567-scie-10-3-e333.xml  
    * 0124-4567-scie-10-3-e333-gf1.jpg  
    * 0124-4567-scie-10-3-e724.pdf  
    * 0124-4567-scie-10-3-e724.xml  
    * 0124-4567-scie-10-3-e724-gf1.jpg  
    * …


    

| Atenção:  Não crie um zip para cada arquivo e nem um relatório xpm.html para arquivo. Cada pacote deve considerar apenas uma pasta zip e um relatório xpm.html para o conjunto de arquivos. Lotes para entrega de periódicos que adotam a modalidade de publicação contínua devem possuir até 5 XMLs; É proibido o uso de underline, ponto, en dash, em dash, acentuação, espaço ou caracteres especiais para nomeação de pastas. |
| :---- |

			

## **Planilha de Other: Controle de Seções** {#planilha-de-other:-controle-de-seções}

A planilha de other é inerente a todos os periódicos indexados nas coleções que adotam a modalidade de publicação contínua e tem o intuito de criar a ordenação dos documentos no sumário online da revista através de um número composto obrigatoriamente por 5 dígitos referente a uma seção e que cria o número a ser inserido no XML na tag [\<article-id\>](#\<article-id\>:-doi-e-other) e deve ser preenchida pelos prestadores XML contratados ou periódicos \- quando estes criam seus próprios XMLs \- antes do envio de cada lote para publicação.

O preenchimento da planilha deve seguir as instruções descritas no [Guia para a Gestão e Criação de Números Other para Ordenação de Seções de Periódicos que Adotam a Modalidade de Publicação Contínua (PC)](https://docs.google.com/document/d/1HvLyoMxBF0WWTbwnDLYGbQmf9Z6OkrL-x8EWaGmRnbg/edit?tab=t.0).

Os responsáveis pelo preenchimento das planilhas de other devem garantir que:

* O envio do lote só ocorra quando a planilha de other estiver preenchida;  
* Todos os arquivos de um lote enviado estejam devidamente preenchidos na planilha de other;  
* A nomeação dos arquivos esteja idêntica ao que foi preenchido na planilha de other e os arquivos que compõem o lote;  
* As seções estejam idênticas nos PDFs e planilha de other;  
* A marcação da seção no XML esteja de acordo (Título e Idioma) com o PDF e planilha de other;  
* O other correspondente a seção esteja corretamente identificado no XML do artigo original em [\<article\>](#\<article\>:-artigo) com a tag [\<article-id pub-id-type="other"\>](#\<article-id\>:-doi-e-other).


O envio de lotes com divergências entre os dados da planilha, PDF e XML podem acarretar solicitação de correção e maior tempo de publicação do lote. 

| Consulte:  [Guia para Implementação da Modalidade de Publicação Contínua em Periódicos Científicos](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf); [Guia para a Gestão e Criação de Números Other para Ordenação de Seções de Periódicos que Adotam a Modalidade de Publicação Contínua (PC)](https://docs.google.com/document/d/1HvLyoMxBF0WWTbwnDLYGbQmf9Z6OkrL-x8EWaGmRnbg/edit?tab=t.0). |
| :---- |

| Consulte no SPS:  [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto). |
| :---- |

## **Como Realizar a Entrega de Artigos para Publicação** {#como-realizar-a-entrega-de-artigos-para-publicação}

As entregas deverão considerar duas ações pelos prestadores de serviço XML:

1. Depósito do pacote de entrega no FTP;  
2. Informação do depósito do pacote de entrega via email.

### **Depósito do Pacote de Entrega no FTP**  {#depósito-do-pacote-de-entrega-no-ftp}

O uso do FTP (*File Transfer Protocol*) de SciELO deve ser realizado preferencialmente pelo programa *FileZilla*. O link para download e as configurações podem ser encontradas no link: \<[http://www.baixaki.com.br/download/filezilla.htm](http://www.baixaki.com.br/download/filezilla.htm)\>.

Existem 2 tipos de conta de FTP:

1. FTP do SciELO;  
   1. Utilizado pelas equipes editoriais que não possuem um prestador com atestado e produzem o pacote XML por conta;  
2. FTP dos prestadores XML;  
   1. Utilizado pelas [Empresas com Atestado de capacidade técnica para serviços de marcação de textos de acordo com *SciELO Publishing Schema* (SciELO PS)](https://www.scielo.org/pt/sobre-o-scielo/parcerias/empresas-com-atestado-de-capacidade-para-marcacao-de-textos/).

Para configurar a conta no programa *FileZilla,* clique em **Arquivo \> Gerenciador de Sites \> Novo Site** e preencha com as informações do login que será utilizado para o upload do pacote XML. Para obter login do FTP SciELO escreva para \<[publicacao@scielo.org](mailto:publicacao@scielo.org)\>, informando qual revista de qual coleção será entregue.

1. **FTP SciELO**  
   1. Após logar neste FTP o pacote .zip deve ser depositado dentro da pasta “[***Acrônimo***](https://docs.google.com/spreadsheets/d/1Sg7PX9eZYfXbKqm7oVnMI-SZ-jxsBJIdwnVJzRPEP1s/edit?gid=0#gid=0) ***Oficial da Revista”*** mais pasta ***Entrega***.   
   2. Se houver necessidade do envio de correções o depósito do pacote deve ocorrer na pasta “[***Acrônimo***](https://docs.google.com/spreadsheets/d/1Sg7PX9eZYfXbKqm7oVnMI-SZ-jxsBJIdwnVJzRPEP1s/edit?gid=0#gid=0) ***Oficial da Revista”** mais pasta* ***Correcao***.  
2. **FTP dos prestadores XML**  
   1. Após logar neste FTP o pacote .zip deve ser depositado dentro da pasta ***Entrega***.   
   2. Se houver necessidade do envio de correções, o depósito do pacote deve ocorrer na pasta ***Correcao***.

Caso ocorra alguma instabilidade no FTP SciELO, o envio poderá ser realizado, **excepcionalmente** por meio do *WeTranfer* \<[https://wetransfer.com/](https://wetransfer.com/)\> ou *SendSpace* \<[https://www.sendspace.com/](https://www.sendspace.com/)\>, no entanto, antes do envio comunique à equipe SciELO Infraestrutura, através da [lista de discussão](https://groups.google.com/forum/#!forum/scielo-xml) para que possam verificar e corrigir o acesso ao FTP. Veja mais informações na seção [SUPORTE](#🔹suporte). O uso neste formato é exclusivo para quando ocorrer problemas de instabilidade com o FTP SciELO. Caso o fornecedor XML encontre problemas de acesso inerentes a sua máquina ou internet, entre em contato com sua equipe de infraestrutura para corrigir o problema.

### **Informação do Depósito do Pacote de Entrega Via Email** {#informação-do-depósito-do-pacote-de-entrega-via-email}

Um e-mail de entrega dos pacotes XML obrigatoriamente deve ser enviado sempre que houver um depósito via FTP (ou envio via *SendSpace* ou *Wetransfer*). O depósito do material novo ou correção precisa ser comunicado à equipe SciELO para que possa ser incluído no fluxo de publicação**.** 

| Atenção:  Só o depósito via *FTP*, *SendSpace* ou *Wetransfer*, não garante a publicação na coleção. |
| :---- |

O email de entrega deve ser enviado para \<[publicacao@scielo.org](mailto:publicacao@scielo.org)\> com cópia para a equipe editorial do periódico para acompanhamento. 

Cada depósito deve ser realizado em pacotes individuais, que correspondem a apenas um volume e/ou número, número especial ou suplemento. Se forem enviados pacotes para diferentes volumes/números, deve-se enviar um e-mail separado para cada depósito.

### **Composição para o Título do Email de Entrega** {#composição-para-o-título-do-email-de-entrega}

O título do email deve conter todos os dados necessários para identificação do pacote e obrigatoriamente deve identificar:

1. Termos oficiais para sinalização de entrega (termo mais pipe):  
   1. Para pacote de atualização \= **Entrega |**  
      1. Pacote de atualização representa documentos que serão publicados em volume do ano corrente ou do ano anterior.  
   2. Para pacote retrospectivo \= **Retrô Entrega |**   
      1. Pacote retrospectivo representa documentos que serão publicados dois anos anteriores ao ano corrente.  
2. Acrônimo oficial do periódico na coleção em caixa baixa (consulte planilha [Acrônimos](https://docs.google.com/spreadsheets/d/1Sg7PX9eZYfXbKqm7oVnMI-SZ-jxsBJIdwnVJzRPEP1s/edit?gid=0#gid=0));  
3. Identificação do volume e/ou número, número especial ou suplemento, com o uso das siglas (em caixa baixa):  
   1. volume \= v  
   2. número \= n  
   3. suplemento \= s  
   4. número especial \= nspe  
4. Indicação do número do lote caso a revista adote a modalidade de publicação contínua (PC), considerando a palavra Lote mais:  
   1. O mínimo de dois ou mais dígitos para a sequência do lote \= 01, 02, 24, 103, etc.  
   2. Dois dígitos finais do ano ao qual pertence o volume e/ou número, número especial ou suplemento, se 2025 \= 25, se 2026 \= 26, etc. Exemplos:  
      1. Lote 3 do volume de 2025 \= Lote 0325  
      2. Lote 47 do volume de 2026 \= Lote 4726  
      3. Lote 103 do volume de 2026 \= Lote 10326  
   3. A sequência de lotes não é anual é a sequência de lotes entregues por número, número especial ou suplemento, iniciando em 01 a cada número.  
5. Ano referente ao volume e/ou número, número especial ou suplemento caso a revista adote a modalidade de publicação regular  
   1. ano com quatro dígitos (YYYY) \= 2025, 2026, etc.  
6. Sigla da coleção, sendo:  
   1. SciELO Brasil \= BR  
   2. SciELO Saúde Pública \= SP  
   3. RevEnf \= RE  
   4. Pepsic \= PS  
      1. Se O periódico é corrente em mais de uma coleçãO, use ambas as siglas separadas por barra /, exemplo:   
         1. BR/SP  
         2. BR/RE  
         3. BR/PS  
      2. Consulte quais coleções um periódico pertence na planilha [Acrônimos](https://docs.google.com/spreadsheets/d/1Sg7PX9eZYfXbKqm7oVnMI-SZ-jxsBJIdwnVJzRPEP1s/edit?gid=0#gid=0).

| Atenção:  A entrega de lotes deve ser realizada de modo sequencial, lote 01 em diante, sem buracos nas entregas; O lote é referente ao volume e/ou número, número especial ou suplemento da entrega, exemplo: v26n1 Lote 01, Lote 02 … / v26n2  Lote 01, Lote 02 … / v26s3  Lote 01, Lote 02 … |
| :---- |

Em resumo o título do email fica, para:

**Publicação Contínua:**

* Entrega ou Retrô Entrega |  \[acrônimo\] v\[x\]n\[x\]s\[x\]nspe\[x\] Lote \[XXXX\] \- \[sigla da coleção\]  
  * *Exemplos:*   
    * Entrega | scie v40 Lote 1625 \- BR  
    * Entrega | scie v40s1 Lote 0325 \- BR/SP  
    * Retrô  Entrega | scie v27n3 Lote 0325 \- SP

**Publicação Regular:**

* Entrega ou Retrô Entrega |  \[acrônimo\] v\[x\]n\[x\]s\[x\]nspe\[x\] \- \[YYYY\] \- \[sigla da coleção\]  
* *Exemplos:*   
  * Entrega | scie v40n2 2025 \- BR  
  * Entrega | scie v40nspe1 2025 \- BR/PS  
  * Retrô Entrega | scie v27n3 1999 \- RE

| Atenção:  Lotes de entrega para periódicos que adotam a modalidade de publicação contínua devem possuir até 5 XMLs.  Pacotes de entrega de periódicos que adotam a modalidade de publicação regular devem possuir o total de XMLs que compõem o número completo (não há restrição para quantidade). |
| :---- |

### **Composição do Corpo do Email de Entrega** {#composição-do-corpo-do-email-de-entrega}

Para o corpo do email, recomenda-se que seja informado pelo menos o seguinte texto:

\------

Informo que o .zip com a marcação XML do periódico “scie v40n2 2025 \- BR”, foi disponibilizado no FTP.

\- Total de XMLs \= xx.

\------

# **🔹FLUXO DE PUBLICAÇÃO DE DOCUMENTOS** {#🔹fluxo-de-publicação-de-documentos}

Esta seção descreve o fluxo institucional de publicação de documentos nas coleções, incluindo as responsabilidades das partes envolvidas, as macroetapas do processo e os status utilizados para identificação da situação de cada pacote entregue para publicação.

Após o envio do email de aviso de depósito dos pacotes XML no FTP para publicação, o fluxo passa a ser integralmente gerido pela equipe de Publicação da unidade de Produção SciELO, por meio do endereço \<[publicacao@scielo.org](mailto:publicacao@scielo.org)\>. Todas as comunicações oficiais relacionadas ao andamento do pacote são realizadas por email, sendo cada etapa identificada por um **status padronizado indicado no título da mensagem**.

Integram o fluxo de publicação:

* **Prestador de serviço XML / Equipe editorial do periódico**: Responsáveis pela análise técnica dos apontamentos, pela realização das correções solicitadas e pela tomada de decisões editoriais quando aplicável;  
* **Equipe de Publicação SciELO**: Responsável pela gestão do fluxo, validação técnica dos pacotes, definição dos status e comunicação oficial com os envolvidos;  
* **Equipe de Tecnologia SciELO**: Responsável pelo processamento técnico dos pacotes e pela atualização dos conteúdos nos sites das coleções SciELO.

O fluxo de publicação compreende, obrigatoriamente, as seguintes macroetapas:

* Depósito do pacote XML no FTP, extração do material e confirmação de Entrega do pacote, com eventual solicitação de correções;  
* Realização do **controle de qualidade (QA)**, contemplando as etapas de **Pré-QA e QA**, com reentrega do pacote quando aplicável;  
* Liberação do pacote para processamento técnico e publicação nos sites das coleções SciELO.

## **Status padronizados dos emails** {#status-padronizados-dos-emails}

Os títulos dos emails enviados no âmbito do fluxo de publicação utilizam status padronizados, conforme descrito a seguir. 

| Consulte no SPS:  [Composição para o Título do Email de Entrega](#composição-para-o-título-do-email-de-entrega), [Cronograma de Processamento de Pacotes para Publicação](#cronograma-de-processamento-de-pacotes-para-publicação). |
| :---- |

* **Entrega | ou Retrô Entrega |**  
  * Indica o depósito de um pacote XML para publicação pelo prestador de serviço. O pacote pode corresponder a:  
    * Pacote de Atualização: volumes do ano corrente ou do ano imediatamente anterior; ou  
    * Pacote Retrospectivo (Retrô): volumes referentes a até dois anos anteriores ao ano corrente.  
* **Entrega Correção |** ou **Retrô Entrega Correção |**  
  * Indica a identificação de inconsistências no depósito, no pacote e/ou no email de entrega, que impedem a inserção do material no fluxo de publicação e exigem correção prévia.  
* **Entrega Confirmada | ou Retrô Entrega Confirmada |**  
  * Indica que o pacote foi recebido pela equipe de Publicação SciELO e devidamente inserido na fila do fluxo de trabalho para publicação.  
* **Pré-QA Correção |** e **Pré-QA Correção Pendente |**  
  * Indica que, durante a etapa de Pré-QA, foram identificadas não conformidades que inviabilizam a disponibilização do pacote no ambiente de homologação para a realização do QA completo, sendo obrigatória a reentrega do pacote pelo prestador de serviço XML.  
  * Quando a reentrega ocorre de forma parcial ou em desacordo com os apontamentos realizados, o email de solicitação de correção é reenviado com a inclusão do termo **Pendente** no título.  
* **Pré-QA Confirmado |**  
  * Indica que as correções solicitadas na etapa de Pré-QA (quando houver), foram entregues em conformidade e que o pacote está apto para prosseguimento à etapa de QA completo no ambiente de homologação.  
  * Pacotes sem solicitação de correção nesta etapa são automaticamente iniciados na etapa de QA, neste caso não há o envio deste email.  
* **QA Correção |** e **QA Correção Pendente |**  
  * Indica que, durante a etapa de QA, foram identificadas não conformidades que impedem a atualização do pacote no site final, sendo obrigatória a reentrega do pacote pelo prestador de serviço XML.  
  * Quando a reentrega ocorre de forma parcial ou em desacordo com os apontamentos realizados, o email de solicitação de correção é reenviado com a inclusão do termo **Pendente** no título.  
* **QA Finalizado |**  
  * Indica a conclusão das etapas de Pré-QA e QA e a aptidão do pacote para ingresso no processamento técnico de atualização no site final, o qual ocorre em até **6 dias úteis** após o envio do email de confirmação.  
* **Cancelamento**  
  * Indica que, após a solicitação de correção nas etapas de Pré-QA ou QA, decorreu prazo superior a **30 dias corridos** sem a reentrega do pacote corrigido. Nessa situação, o pacote é cancelado e deverá ser reenviado, sem a preservação da posição anteriormente ocupada na fila de processamento.  
* **Cancelamento Prestador/Revista**  
  * Indica o cancelamento do pacote por solicitação do prestador de serviço XML ou da equipe editorial do periódico, para realização de correções específicas, alterações de dados ou outros ajustes. O pacote deverá ser reenviado, sem a preservação da posição anteriormente ocupada na fila de processamento.  
* **Entrega | … Reenvio de Pacote Cancelado |** ou **Retrô Entrega | … Reenvio de Pacote Cancelado**  
  * Indica o novo depósito de um pacote previamente cancelado. Nesses casos, o pacote retorna ao fluxo de publicação desde a etapa inicial, sendo inserido novamente na fila do fluxo de trabalho para publicação.

| Atenção:  Não é permitida a realização de correções ou alterações em pacotes que já tenham ingressado no processamento técnico; Pacotes já processados ou atualizados no site final podem ser objeto de correção, exceto quando a alteração envolver dados que exijam a publicação de [Errata](#errata), [Retratação](#retratação) ou [Adendo](#adendo), nos termos das diretrizes editoriais vigentes; A solicitação de correção pontual de pacote já publicado deverá ser realizada, obrigatoriamente, por meio de resposta ao email “QA Confirmado” do pacote que contém o arquivo. As solicitações recebidas serão analisadas pela equipe de Publicação SciELO, que definirá o encaminhamento cabível, de acordo com as normas e procedimentos institucionais. A solicitação deve conter, de forma clara e objetiva: A descrição do ajuste solicitado; A identificação do arquivo envolvido; e O link do documento publicado no SciELO. |
| :---- |

## **Tabela-síntese de status do fluxo de publicação** {#tabela-síntese-de-status-do-fluxo-de-publicação}

A tabela a seguir apresenta um resumo dos status utilizados nos títulos dos emails ao longo do fluxo de publicação, com suas respectivas descrições e implicações operacionais.

| Status do Email | Etapa do Fluxo | Descrição Resumida | Ação Esperada |
| ----- | :---: | :---: | :---: |
| **Entrega** **Retrô Entrega** | Depósito inicial | Depósito de pacote XML para publicação (atualização ou retrospectivo). | Aguardar validação e confirmação de entrega pela equipe de Publicação SciELO. |
| **Entrega Correção**  **Retrô Entrega Correção** | Depósito inicial | Identificação de inconsistências no depósito, no pacote ou no email de entrega. | Corrigir e reenviar o pacote conforme orientações. |
| **Entrega Confirmada** **Retrô Entrega Confirmada** | Fila de publicação | Pacote recebido e inserido na fila do fluxo de trabalho. | Aguardar início das etapas de controle de qualidade. |
| **Pré-QA Correção** | Pré-QA | Não conformidades identificadas que impedem o avanço para QA. | Corrigir e reenviar o pacote. |
| **Pré-QA Correção Pendente** | Pré-QA | Reentrega parcial ou incorreta das correções solicitadas em Pré-QA. | Revisar apontamentos e reenviar o pacote corrigido integralmente. |
| **Pré-QA Confirmado** | Pré-QA | Correções atendidas e pacote apto para QA completo. | Aguardar resultado do QA. |
| **QA Correção** | QA | Não conformidades identificadas que impedem a atualização no site final. |  Corrigir e reenviar o pacote. |
| **QA Correção Pendente** |  QA | Reentrega parcial ou incorreta das correções solicitadas em QA. | Revisar apontamentos e reenviar o pacote corrigido integralmente. |
| **QA Finalizado** | Pós-QA | Pré-QA e QA concluídos, pacote apto para processamento técnico. | Aguardar processamento e publicação no site final. |
| **Cancelamento Cancelamento Prestador/Revista** | Interrupção do fluxo | Ausência de reentrega após 30 dias corridos da solicitação de correção. ou Cancelamento solicitado pelo prestador ou pela revista. | Reenviar o pacote como nova entrega, sem preservação de posição na fila. |
| **Entrega** … **\- Reenvio de Pacote Cancelado** **Retrô Entrega** … **\- Reenvio de Pacote Cancelado** | Reinício do fluxo | Novo depósito de pacote previamente cancelado. | Pacote retorna ao fluxo desde a etapa inicial. |

# **🔹TEMPO DE PUBLICAÇÃO**  {#🔹tempo-de-publicação}

| Atenção:  É importante reforçar que a equipe de publicação não informa uma data ou previsão para a publicação dos pacotes entregues. |
| :---- |

O prazo de publicação de pacotes de atualização sem correção, ocorre em até **15 úteis a partir do início do controle de qualidade (QA)** pela equipe de publicação. 

- O início do QA vai depender do montante de entregas e processamentos no período em relação a capacidade operacional da unidade, ou seja, após a entrega do pacote o início pode não ser imediato.

O prazo de publicação pode aumentar quando houver feriados, feriados prolongados, eventos SciELO (em que a equipe de publicação estará ausente), recesso de fim de ano ou quando o pacote entregue possuir correções (Pré QA, QA Geral ou correções solicitadas durante o processo de QA pela revista ou prestador), o que também leva em consideração o tempo em que o pacote corrigido é devolvido pelo prestador XML incluindo correções que precisam antes passar pela equipe editorial do periódico.

O prazo de publicação de pacotes retrospectivos será realizado gradualmente, de acordo com o fluxo paralelo de publicação de periódicos retrospectivos, de modo a não comprometer a publicação de pacotes de atualização. Sendo assim, não há um prazo oficial para a publicação de pacotes retrospectivos.

As revistas que adotam a modalidade contínua (PC), devem seguir as regras desta modalidade para sempre adiantar a publicação. Recomenda-se que os periódicos **encerrem** a publicação dos artigos do ano corrente até **outubro** e obrigatoriamente para a coleção SciELO Brasil só **abram** o volume/números do ano seguinte a partir de **novembro**. Para mais informações consulte a seção [Antecipação e Restrições de Publicação na Modalidade Contínua.](#antecipação-e-restrições-de-publicação-na-modalidade-contínua)

Para as revistas que adotam a publicação em modalidade regular (baseada em números fechados) recomenda-se que os números sejam publicados no primeiro mês da periodicidade de cada número, evitando assim atrasos na publicação.

| Consulte:  [Guia para Implementação da Modalidade de Publicação Contínua em Periódicos Científicos](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf). |
| :---- |

| Consulte no SPS:  [Antecipação e Restrições de Publicação na Modalidade Contínua](#antecipação-e-restrições-de-publicação-na-modalidade-contínua). |
| :---- |

## **Cronograma de Processamento de Pacotes para Publicação** {#cronograma-de-processamento-de-pacotes-para-publicação}

Os pacotes prontos para a publicação passam por processamento realizado pela equipe de tecnologia SciELO que carrega o conteúdo dos pacotes para os sites finais das coleções. Os processamentos ocorrem **2 vezes por semana**  (exceto feriados) seguindo o cronograma:

* **Processamento realizado às terças-feiras:** Atualizam os documentos as sextas-feiras a partir das 16h00.  
* **Processamento realizado às sextas-feiras:** Atualizam os documentos as segundas-feiras a partir das 16h00.

# 

# **🔹Encoding e \<\!DOCTYPE\>**  {#🔹encoding-e-<!doctype>}

O **Encoding** especifica a codificação de caracteres usada no texto do documento. Para o [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema) obrigatoriamente todos os XMLs devem ser codificados em UTF-8. A especificação do padrão XML ([2.8 Prolog and Document Type Declaration](https://www.w3.org/TR/xml/#sec-prolog-dtd)) fornece mais informações sobre as características de codificação requeridas para este padrão.

A declaração **\<\!DOCTYPE\>** indica a DTD à qual o XML encontra-se associado, ou seja, define as regras estruturais do documento. O SciELO Publishing Schema ([SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema)) na sua versão 1.10, utiliza como base o padrão Journal Publishing Tag Library (JATS) na versão [1.3](https://jats.nlm.nih.gov/publishing/tag-library/1.3/). 

**Exemplo:**

```
<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE article PUBLIC "-//NLM//DTD JATS (Z39.96) Journal Publishing DTD v1.3 20210610//EN" "https://jats.nlm.nih.gov/publishing/1.3/JATS-journalpublishing1-3.dtd">
```

| Consulte:  [2.8 Prolog and Document Type Declaration](https://www.w3.org/TR/xml/#sec-prolog-dtd). |
| :---- |

# **🔹CODIFICAÇÃO DE CARACTERES ESPECIAIS** {#🔹codificação-de-caracteres-especiais}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

Caracteres especiais, quando utilizados, devem ser inseridos diretamente no documento ou por meio de referências numéricas em notação hexadecimal. Por exemplo, o caractere sigma maiúsculo deve ser representado por Σ ou &\#x03A3;.

Não é permitido o uso de referências a caracteres de uso privado da tabela Unicode contidas no intervalo xE000 – xF8FF.

* Para a leitura correta de caracteres especiais por leitores de tela ou outros sistemas de texto para fala ou texto para braille, certifique-se de usar o valor Unicode correto, em vez de qualquer caractere que pareça semelhante ao caractere desejado. Por exemplo, ao indicar temperatura, tome cuidado para usar o símbolo de grau (°, &\#176;) e não um “o” minúsculo (°, &\#7506;) ou o símbolo ordinal masculino (º, &\#186;).

A codificação de caracteres pode ser observada em: [Tabela Unicode](https://symbl.cc/en/unicode-table/).

Caracteres que compõem tags, atributos, elementos e codificação, **obrigatoriamente** devem ser codificados para criar um XML válido:

| Caractere | Entidade | Descrição |
| :---: | :---: | ----- |
| “ | \&quot; | Aspas como valor de atributo de elemento no XML. |
| ‘ | \&apos; | Apóstrofo como valor de atributo de elemento no XML. |
| & | \&amp; | & como valor de atributo ou texto de elemento no XML. |
| \< | \&lt; | \< como valor de atributo ou texto de elemento no XML. |
| \> | \&gt; | \> como valor de atributo ou texto de elemento no XML. |

| Consulte:  [Tabela Unicode](https://symbl.cc/en/unicode-table/). |
| :---- |

# **🔹MARCAÇÃO PARA ACESSIBILIDADE** {#🔹marcação-para-acessibilidade}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

A marcação para acessibilidade documenta as melhores práticas [XML-SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema) a fim de garantir acesso à informação para usuários com deficiências físicas ou situacionais e para os dispositivos que os auxiliam, com o objetivo de tornar o uso do XML acessível para versões de apresentação em HTML do documento para o usuário final.

Três objetos são o foco da marcação [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema) para acessibilidade:

1. **Figura/Imagem**  
2. **Vídeo**  
3. **Áudio**

Para o [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema), a marcação de acessibilidade para figura, imagem, vídeo e áudio ocorre com o uso de 3 elementos:

| Elemento XML | Descrição |
| :---: | ----- |
| **\<alt-text\>** | Descreve de forma breve, mas significativa, uma imagem, figura, vídeo ou áudio. |
|  **\<long-desc\>** | Descreve de forma detalhada uma imagem, figura, vídeo ou áudio que contém grau de complexidade. É especialmente utilizado para vídeo, áudio e gráficos. |
|  **\<sec sec-type="transcript"\>** | Representa a transcrição de um vídeo ou áudio. Uma transcrição é um texto que transmite o áudio falado na íntegra e descrições de sons ou ações na gravação. |

[\<alt-text\>](#\<alt-text\>) e [\<long-desc\>](#\<long-desc\>) podem ocorrer simultaneamente em [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura), [\<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) ou [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) e só podem comportar conteúdos de texto, números e caracteres especiais.

Se uma imagem for apenas decorativa (decisão editorial), use **\<alt-text\>null\</alt-text\>**. Este uso permitirá que leitores de tela pulem imagens incluídas em um documento apenas por razões estéticas ou aquelas que são ilustrações do texto já fornecido e marcado no XML. 

Esta seção documenta apenas a marcação XML e não tem relação com a apresentação destes dados nos conteúdos originais PDF, word, site, etc. O formato desta apresentação nos conteúdos originais PDF deve ser definido junto à equipe editorial do periódico e recomenda-se a divulgação das práticas adotadas, em suas políticas editoriais de acesso público, a autores, revisores e leitores.

Periódicos que adotam a marcação XML para acessibilidade em seus documentos devem informar toda a cadeia editorial, em especial os prestadores de serviço XML que marcam os conteúdos dos documentos que são publicados em SciELO. Pode ser requerido documento com as descrições e sua relação com cada objeto do documento.

É recomendável que documentos em XML usem a marcação para acessibilidade em sua totalidade, quando aplicável, e não apenas em partes selecionadas.

A descrição de uma imagens deve estar alinhada ao seguinte conceito:

* Descrição com base em: o quê /quem \+ onde \+ como \+ faz o quê \+ como \+ quando \+ de onde, (em resumo: formato \+ sujeito \+ paisagem \+ contexto \+ ação), sendo:   
  * **O quê / Quem:** Identificar o sujeito, objeto ou cena a ser descrita;  
  * **Onde:** Localizar o sujeito, objeto ou cena a ser descrita;  
  * **Como:** Empregar adjetivos para qualificar o sujeito, objeto ou cena da descrição;  
  * **Faz o quê / Como:** Empregar verbos para descrever a ação e advérbio para descrever as circunstâncias da ação;  
  * **Quando:** Utilizar o advérbio para referenciar o tempo em que ocorre a ação;  
  * **De onde:** Identificar os diversos enquadramentos da imagem. 

As melhores práticas para descrição de figuras e imagens segundo [Rogers, et al. (2025)](https://scholarlykitchen.sspnet.org/wp-content/uploads/2025/08/Beyond-OA-article-image-description-recommendation-packet.pdf) são:

Descrições de imagens devem ser:

* **Precisas e contextuais:** Descrever claramente o conteúdo da imagem e sua relevância em relação ao texto.  
* **Concisas, mas informativas:** Fornecer detalhes suficientes para transmitir a informação essencial sem ser excessivamente prolixo.  
* **Orientadas para o propósito:** Destacar o significado e a função do elemento visual, e não apenas suas características visuais.  
* **Relevantes e complementares:** Estar alinhadas à mensagem central do texto, de modo a ampliar a compreensão geral.

Armadilhas para evitar [(Rogers, et al., 2025\)](https://scholarlykitchen.sspnet.org/wp-content/uploads/2025/08/Beyond-OA-article-image-description-recommendation-packet.pdf):

* **Não dependa exclusivamente das figuras.** Informações importantes devem sempre estar incluídas no texto ao redor. O texto alternativo é um complemento, não um substituto do contexto.  
* **Evite redundância.** Leitores de tela indicam que o texto alternativo substitui a imagem, portanto não use expressões como “Imagem de…” ou “Gráfico de…”.  
* **Não repita a legenda.** O texto alternativo deve fornecer contexto adicional, não duplicar o que já está na legenda ou no texto ao redor.  
* **Evite detalhes irrelevantes.** Informações que não aparecem na figura (como autor, data, fonte ou referências bibliográficas) não pertencem ao texto alternativo.  
* **Não interprete a imagem.** O texto alternativo deve descrever o que está visível, sem oferecer interpretações subjetivas ou opiniões.  
* **Não sobrecarregue com texto.** Mantenha o texto alternativo conciso. Evite descrições longas que possam cansar ou confundir o leitor.  
* **Evite formatações.** Leitores de tela não interpretam formatação (por exemplo, listas com marcadores). Prefira descrições simples e diretas.  
* **Não presuma contexto visual.** Descreva a imagem e sua função na publicação como se o leitor não pudesse “ver”. Evite frases como “Como você pode ver…”.  
* **Evite pressupostos de gênero.** Seja específico. Em vez de “Homem”, use “Pessoa sorridente lendo um livro”. Não é possível afirmar o gênero; use termos como “Pessoa” ou “Indivíduo”.  
* **Não esqueça de testar.** Sempre verifique seu texto alternativo com ferramentas como leitores de tela para garantir que a mensagem seja transmitida de forma eficaz. Combine com testes de usabilidade, grupos focais e avaliação humana.

**10 dicas para texto alternativo \<alt-text\>**

Um texto alternativo bem elaborado amplia a acessibilidade. Torne seu conteúdo mais inclusivo para os leitores [(Rogers, et al., 2025\)](https://scholarlykitchen.sspnet.org/wp-content/uploads/2025/08/Beyond-OA-article-image-description-recommendation-packet.pdf).

1. **Pense no porquê.**  
   Decida a melhor forma de transmitir a alguém o que é essa imagem, mesmo que nunca a tenha “visto” antes. Por que ela é relevante e o que essa descrição acrescenta ao conteúdo geral?

2. **Seja conciso.**  
   Evite prolixidade e seja claro ao fornecer os detalhes. Transmita uma mensagem eficaz em poucas palavras — não mais que uma ou duas frases, ou menos de 120 caracteres. Leitores de tela podem interromper a leitura nesse ponto e cortar a descrição. Use palavras-chave descritivas. Revise ortografia e gramática.  
   1. *Exemplos:*  
      1. “Ilustração de uma equipe diversa colaborando em um projeto.”  
      2. “Close-up de um girassol em flor.”

3. **Capte emoções.**  
    Mostre o tom emocional da imagem quando for relevante.  
   1.  *Exemplo:*  
      1. “Crianças felizes brincando em um parque ensolarado.”

4. **Forneça contexto.**  
   Adicione contexto à descrição para torná-la significativa. O texto alternativo deve oferecer o enquadramento adequado para dar vida à narrativa.  
   1. *Exemplos:*  
      1. “Máquina de escrever vintage sobre uma mesa de madeira, com filtro sépia para parecer uma foto antiga.”  
      2. “Máquina de escrever do meu avô sobre uma mesa de madeira.”

5. **Evite repetição.**  
   Concentre-se nos aspectos únicos da imagem.  
   1. *Exemplo:*  
      1. “Cão golden retriever pegando um frisbee no ar.”

6. **Destaque os elementos principais.**  
   Descreva o que chama atenção e seja específico.  
   1. *Exemplo:*  
      1. “CEO discursando em um auditório lotado.”

7. **Seja descritivo, não prescritivo.**  
   Descreva o que está acontecendo sem dizer às pessoas como interpretar a imagem.  
   1. *Exemplo:*  
      1. “Obra de arte abstrata com cores vibrantes e linhas fluidas.”

8. **Use linguagem ativa.**  
   1. *Exemplo:*  
      1. “Trilheiro subindo por uma trilha montanhosa acidentada.”

9. **Considere cores e contraste.**  
   Lembre-se de que algumas pessoas nunca viram cores e não têm referência visual.  
   1. *Exemplo:*  
      1. “Retrato em preto e branco de alto contraste de um artista idoso.”

10. **Teste com leitores de tela e ferramentas.**  
    Use ferramentas como geradores de texto alternativo e leitores de tela — incluindo extensões de navegador como o Google Lighthouse — para verificar como o texto é exibido. Revise e atualize sempre que necessário. O contexto é fundamental. A Universidade de [Harvard disponibiliza bons exemplos de ferramentas gratuitas](https://accessibility.huit.harvard.edu/auto-tools-testing#free). Siga o [Alt Decision Tree do W3C](https://www.w3.org/WAI/tutorials/images/decision-tree/) como guia e pergunte-se: faz sentido? Está claro e envolvente?

| Consulte:  [Guest Post – Beyond Open Access, Part II: Make Images Truly Accessible for All](https://scholarlykitchen.sspnet.org/2025/08/27/guest-post-beyond-open-access-part-ii-make-images-truly-accessible-for-all/?informz=1&nbd=256a655b-c5d3-4c7a-9450-f26e9f534ebd&nbd_source=informz); [Infographics: Guest Post – Beyond Open Access, Part II: Make Images Truly Accessible for All](https://scholarlykitchen.sspnet.org/wp-content/uploads/2025/08/Beyond-OA-article-image-description-recommendation-packet.pdf); [Harvard University: Free Automated Tools](https://accessibility.huit.harvard.edu/auto-tools-testing#free); [W3C: An alt Decision Tree](https://www.w3.org/WAI/tutorials/images/decision-tree/); [W3C: Web Accessibility Initiative on Decorative Images](https://www.w3.org/WAI/tutorials/images/decorative/); [Boas práticas de acessibilidade digital](https://mwpt.com.br/acessibilidade-digital/boas-praticas/); [Nota Técnica Nº 21 / 2012 do MEC](https://acessibilidadeinformacional.paginas.ufsc.br/files/2021/12/Nota-T%C3%A9cnica-n%C2%BA-21-de-descri%C3%A7%C3%A3o-de-imagem.pdf). |
| :---- |

    

| Consulte na JATS *(Esta seção é baseada nas documentações)*:  [JATS4R \- Recommendations: Accessibility](https://jats4r.niso.org/accessibility/);	 [Journal Publishing Tag Library NISO JATS Version 1.3 (ANSI/NISO Z39.96-2021): Accessibility](https://jats.nlm.nih.gov/publishing/tag-library/1.3/chapter/accessibility.html#pub-accessibility); [Journal Publishing Tag Library NISO JATS Version 1.3 (ANSI/NISO Z39.96-2021): Accessibility Class Elements](https://jats.nlm.nih.gov/publishing/tag-library/1.3/pe/access.class.html). |
| :---- |

| Consulte no SPS:  [\<alt-text\>](#\<alt-text\>); [\<long-desc\>](#\<long-desc\>); [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). |
| :---- |

    

## **\<alt-text\>** {#<alt-text>}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) | Zero ou uma vez |
| [\<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) | Zero ou uma vez |
| [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) | Zero ou uma vez |
| [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) | Zero ou uma vez |

Quando o conteúdo descritivo for gerado por máquina, o elemento deve conter o atributo @content-type com o seguinte valor:

| @content-type | Descrição |
| :---: | ----- |
| **machine-generated** | Conteúdo de [\<alt-text\>](#\<alt-text\>) gerado por máquina. Exemplo: IA, softwares geradores de conteúdo, etc. |

[\<alt-text\>](#\<alt-text\>) deve ser usado para descrever uma figura ou imagem estática de forma breve **até 120 caracteres** (para descrição acima de 120 caracteres use [\<long-desc\>](#\<long-desc\>)) com informações pertinentes ao objeto. O conteúdo de [\<alt-text\>](#\<alt-text\>) não deve ser usado para substituir ou copiar a informação descrita em \<label\> ou \<caption\>.

O elemento deve ser usado apenas quando houver a ocorrência de [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura), [\<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura), [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) ou [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) e deve estar contido dentro destes elementos e ocorrendo uma única vez. Exemplo da estrutura: 

* \<graphic\>**\<alt-text\>**descrição**\</alt-text\>**\</graphic\>   
* \<inline-graphic\>**\<alt-text\>**descrição**\</alt-text\>**\</inline-graphic\>  
* \<media\>**\<alt-text\>**descrição**\</alt-text\>**\</media\>   
* \<inline-media\>**\<alt-text\>**descrição**\</alt-text\>**\</inline-media\>

Em [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) e [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) o elemento [\<alt-text\>](#\<alt-text\>) só deverá ocorrer quando o formato do objeto for vídeo ou áudio, tendo os seguintes atributos para @mime-type e @mime-subtype:

| @mime-type | @mime-subtype |
| :---: | :---: |
| video | mp4 |
| audio | mp3 |

Para vídeo e áudio recomenda-se preferencialmente o uso de [\<long-desc\>](#\<long-desc\>), a não ser que os objetos tenham um curto tempo de duração e 120 caracteres consigam suprir de forma eficiente a descrição informacional do conteúdo.

**Exemplos**:

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) **\+ [\<alt-text\>](#\<alt-text\>)** em [figura](#\<fig\>:-figura)

```
<fig id="f1">
    <label>Figura 1</label>
    <caption>
        <title>Título da Figura</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg">
   	 <alt-text content-type="machine-generated">Breve descrição do objeto (até 120 caracteres)</alt-text>
    </graphic>
</fig>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) **\+ [\<alt-text\>](#\<alt-text\>)** em figura de [material suplementar](#\<supplementary-material\>:-material-suplementar)

```
<sec sec-type="supplementary-material" id="sec1">
  <title>Supplementary Materials</title>        
<supplementary-material id="suppl1">
<label>Supplementary material 1</label>
    		<caption>
       		 <title>Figure 1</title>
   		</caption>
   		<graphic xlink:href="1234-5678-scie-58-e1043-gf3.jpg">
 		  	<alt-text>Breve descrição do objeto (até 120 caracteres)</alt-text>
		  </graphic>
 </supplementary-material> 
</sec>
```

[**\<inline-graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) **\+ [\<alt-text\>](#\<alt-text\>)** em [figura no parágrafo](#\<graphic\>-e-\<inline-graphic\>:-figura)

```
<p>Phasellus ac iaculis nisl. Integer dictum odio et tristique semper suspendisse potenti, maecenas <inline-graphic xlink:href="1234-5678-scie-58-e1043-gf17.jpg"><alt-text content-type="machine-generated">Breve descrição do objeto (até 120 caracteres)</alt-text></inline-graphic> consectetur fermentum nisi eu commodo. Phasellus at mollis exvivamus sit amet imperdiet est.</p>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) **\+ [\<alt-text\>](#\<alt-text\>)** em [resumo visual](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief)

```
<abstract abstract-type="graphical">
  <title>Visual Abstract</title>
    <p>
      <fig id="vs1">
         <caption>
            <title>Título</title>
         </caption>
         <graphic xlink:href="1234-5678-scie-58-e1043-vs1.jpg">
<alt-text>Breve descrição do objeto (até 120 caracteres)</alt-text>
	    </graphic>
      </fig>
    </p>
</abstract>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) **\+ [\<alt-text\>](#\<alt-text\>)** em figura decorativa (Valor obrigatório \= null)

```
<fig id="f1">
       <graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg">
   	 <alt-text>null</alt-text>
    </graphic>
</fig>
```

[**\<media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia) **\+ [\<alt-text\>](#\<alt-text\>)** em áudio de curta duração

```
<media mimetype="audio" mime-subtype="mp3" xlink:href="1234-5678-scie-58-e1043-md1.mp3">
<label>Audio 1</label>
 		<caption>
 			 <title>título</title> 
 </caption> 
<alt-text>Breve descrição do objeto (até 120 caracteres)</alt-text>
</media>
```

| Atenção:  Não use [\<alt-text\>](#\<alt-text\>) para outros fins que não sejam acessibilidade (breve descrição de uma imagem ou figura), como, por exemplo, figuras sem declaração de \<caption\> ou \<label\>. |
| :---- |

| Consulte no SPS:  [\<media\> e \<inline-media\>: Objeto Multimídia](#\<media\>-e-\<inline-media\>:-objeto-multimídia); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [Nomeação de Arquivos](#nomeação-de-arquivos); [\<long-desc\>](#\<long-desc\>); [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). |
| :---- |

## **\<long-desc\>** {#<long-desc>}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) | Zero ou uma vez |
| [\<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) | Zero ou uma vez |
| [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) | Zero ou uma vez |
| [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) | Zero ou uma vez |

Quando o conteúdo descritivo for gerado por máquina, o elemento deve conter o atributo @content-type com o seguinte valor:

| @content-type | Descrição |
| :---: | ----- |
| **machine-generated** | Conteúdo de [\<long-desc\>](#\<long-desc\>) gerado por máquina. Exemplo: IA, softwares geradores de conteúdo, etc. |

Este elemento informa uma descrição textual detalhada do objeto visual, com a intenção de transmitir as mesmas informações que um usuário sem deficiências físicas ou situacionais assimilaria ao olhar para o objeto. Obrigatoriamente deve possuir conteúdo textual **acima de 120 caracteres** (para descrição de até 120 caracteres use [\<alt-text\>](#\<alt-text\>)). O conteúdo de [\<long-desc\>](#\<long-desc\>) não deve ser usado para substituir ou copiar a informação descrita em \<label\> ou \<caption\>.

O elemento deve ser usado apenas quando houver a ocorrência de [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura), [\<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura), [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) ou [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) e deve estar contido dentro destes elementos e ocorrendo uma única vez. Exemplo da estrutura: 

* \<graphic\>**\<long-desc\>**descrição**\</long-desc\>**\</graphic\>   
* \<inline-graphic\>**\<long-desc\>**descrição**\</long-desc\>**\</inline-graphic\>  
* \<media\>**\<long-desc\>**descrição**\</long-desc\>**\</media\>   
* \<inline-media\>**\<long-desc\>**descrição**\</long-desc\>**\</inline-media\>

Em [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) e [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) o elemento [\<long-desc\>](#\<long-desc\>) só deverá ocorrer quando o formato do objeto for vídeo ou áudio, tendo os seguintes atributos para @mime-type e @mime-subtype:

| @mime-type | @mime-subtype |
| :---: | :---: |
| video | mp4 |
| audio | mp3 |

**Exemplos:**

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) **\+ [\<long-desc\>](#\<long-desc\>)** em [figura](#\<fig\>:-figura)

```
<fig id="f1">
    <label>Figura 1</label>
    <caption>
        <title>Título da Figura</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg">
   	 <long-desc content-type="machine-generated">Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
    </graphic>
</fig>
```

[**\<media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia) **\+ [\<long-desc\>](#\<long-desc\>)** em [material suplementar](#\<supplementary-material\>:-material-suplementar)

```
<supplementary-material id="suppl1">
<label>Supplementary material 1</label>
            <caption>
                <title>Video 1</title>
            </caption>
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<long-desc content-type="machine-generated">Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
</media>
</supplementary-material>
```

[**\<media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia) **\+ [\<long-desc\>](#\<long-desc\>)** em vídeo

```
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<label>Video 1</label>
 		<caption>
 			 <title>Vídeo: malesuada vehicula</title> 
 </caption> 
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
</media>
```

[**\<inline-media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia) **\+ [\<long-desc\>](#\<long-desc\>)** em aúdio em parágrafo

```
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque dui eros, laoreet eget sem nec, cursus vulputate tellus <inline-media mimetype="audio" mime-subtype="mp3" xlink:href="1234-5678-scie-58-e1043-md1.mp3">Vídeo<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc><inline-media>, elit erat malesuada magna, in tempor urna nunc eget leo.</p>
```

| Atenção:  Não use [\<long-desc\>](#\<long-desc\>) para outros fins que não sejam acessibilidade (descrição detalhada de uma imagem ou figura), como, por exemplo, figuras sem declaração de \<caption\> ou \<label\>. |
| :---- |

| Consulte no SPS:  [\<media\> e \<inline-media\>: Objeto Multimídia](#\<media\>-e-\<inline-media\>:-objeto-multimídia); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [Nomeação de Arquivos](#nomeação-de-arquivos); [\<alt-text\>](#\<alt-text\>); [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). |
| :---- |

## **\<sec sec-type="transcript"\>** {#<sec-sec-type="transcript">}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| \<body\> | Zero ou mais vezes |
| \<back\> | Zero ou mais vezes |

Atributos obrigatórios para [\<sec\>](#\<sec\>:-seção-de-texto):

1. @sec-type="transcript"   
2. @id

Atributos obrigatórios para [\<xref\>](#\<xref\>:-referência-cruzada):

1. @ref-type="sec"   
2. @id

A seção de transcrição é usada para transcrever um texto que transmite o áudio falado e gravação de vídeo na íntegra com suas descrições de sons e ações. Recomenda-se que vídeos e áudios sempre venham acompanhados de seções de transcrição e não apenas [\<alt-text\>](#\<alt-text\>) e/ou [\<long-desc\>](#\<long-desc\>).

O elemento [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>) só poderá ocorrer na presença dos elementos [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) e [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) quando o formato do objeto for vídeo ou áudio, tendo os seguintes atributos para @mime-type e @mime-subtype: 

| @mime-type | @mime-subtype |
| :---: | :---: |
| video | mp4 |
| audio | mp3 |

[\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) e [\<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) devem incluir uma referência cruzada \<xref ref-type=”sec”\>, que vincula a transcrição com a seção de transcrição [\<sec sec-type=”transcript”\>](#\<sec-sec-type="transcript"\>). A seção poderá ocorrer em \<body\> ou \<back\> obrigatoriamente identificando um \<title\>.

**Exemplo:** referência cruzada [\<xref\>](#\<xref\>:-referência-cruzada) para seção de transcrição [\<sec\>](#\<sec\>:-seção-de-texto) em [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia)

```
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<label>Interview with Gabriel and Denise</label> 		
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
<xref ref-type="sec" rid="TR1"/>
</media>
```

Cada objeto \- vídeo e áudio, contido no documento deve possuir sua própria seção de transcrição. Na ocorrência de mais de uma seção de transcrição atribua @id diferentes (TR1, TR2, etc.).

O texto da transcrição em [\<sec sec-type=”transcript”\>](#\<sec-sec-type="transcript"\>) deve ser marcado com \<p\> e quando o diálogo/discussão ocorrer entre duas ou mais entidades, deverão ser utilizados os seguintes elementos:

| Elemento | Descrição |
| :---: | ----- |
| [\<speaker\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/speaker.html) | O orador do diálogo ou discurso \<speech\> |
| [\<speech\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/speech.html) | Texto proferido pelo orador \<speaker\> |

**Exemplo:** diálogo entre duas pessoas \- Gabriel e Denise.

[**\<sec sec-type="transcript"\>**](#\<sec-sec-type="transcript"\>) **\+ [\<speaker\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/speaker.html) \+ [\<speech\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/speech.html)**

```
<sec sec-type="transcript" id="TR1">
<title>Interview with Gabriel and Denise</title>
<p>Nam convallis dolor sed ligula mollis vulputate. Mauris id felis id erat bibendum aliquam nec quis nulla. Sed nec augue orci. Donec rhoncus justo vitae enim finibus luctus. Praesent iaculis, velit iaculis efficitur accumsan, ex ligula elementum ipsum, et laoreet velit odio id nibh:</p>
<speech>
<speaker>Gabriel</speaker>
<p>Etiam ac arcu at nunc lacinia fermentum. Ut molestie vestibulum lacus, at ultricies orci ravida eget. Maecenas pellentesque leo ut sem cursus dictum. Nam at tempus arcu.</p>
</speech>
<speech>
<speaker>Denise</speaker>
<p>Pellentesque at bibendum nibh. Vestibulum non justo in nibh lobortis viverra eu eu magna. Etiam porta mollis libero, ut tempus est dictum eget. Vestibulum interdum leo vel dui malesuada, ac interdum arcu pharetra</p>
</speech>
<speech>
<speaker>Gabriel</speaker>
<p>Sed placerat dolor tellus</p>
</speech>
</sec>
```

| Consulte na JATS:  [\<speaker\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/speaker.html); [\<speech\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/speech.html). |
| :---- |

| Consulte no SPS:  [\<media\> e \<inline-media\>: Objeto Multimídia](#\<media\>-e-\<inline-media\>:-objeto-multimídia); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [Nomeação de Arquivos](#nomeação-de-arquivos); [\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada); [\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<alt-text\>](#\<alt-text\>); [\<long-desc\>](#\<long-desc\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). |
| :---- |

## **\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada** {#<long-desc>,-<alt-text>-e-<sec-sec-type="transcript">:-principais-características-e-marcação-combinada}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

Os três elementos têm como objeto central a acessibilidade, no entanto, o uso na marcação deve levar em consideração seus propósitos e para isso, podem ser usados de forma combinada nos objetos digitais permitidos. O uso de um, de dois ou dos três elementos fica a critério de decisão editorial. As principais características das tags são:

| \<alt-text\> | \<long-desc\> | \<sec sec-type="transcript"\> |
| ----- | ----- | ----- |
| Até 120 caracteres. | Mais de 120 caracteres. | Não tem restrição de caracteres. |
| Descrição curta de um objeto. | Descrição longa de um objeto. | Descrição na íntegra. |
| Descreve forma. | Descreve forma e significado. | Transcreve o conteúdo. |
| Mais usado para figura e imagem simples, mas pode ser usado para vídeo e áudio, em especial de curta duração. | Pode ser usado para figura,  imagem, vídeo e áudio, em especial quando o objeto apresentar grau de complexidade. | Usado exclusivamente para vídeo e áudio. |
| Usualmente é um elemento visual de apresentação. | Usualmente não é um elemento visual de apresentação. | Usualmente é um elemento visual de apresentação. |

Para áudios e vídeos de longa duração recomenda-se o uso de [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>) e opcionalmente o uso combinado com o elemento [\<long-desc\>](#\<long-desc\>) e/ou [\<alt-text\>](#\<alt-text\>).

**Exemplos de marcação combinada:**

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) **\+ [\<alt-text\>](#\<alt-text\>) e [\<long-desc\>](#\<long-desc\>)** em [figura](#\<fig\>:-figura)

```
<fig id="f1">
    <label>Figura 1</label>
    <caption>
        <title>Título da Figura</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg">
       <alt-text content-type="machine-generated">Breve descrição do objeto (até 120 caracteres)</alt-text>
   	 <long-desc content-type="machine-generated">Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
    </graphic>
</fig>
```

[**\<media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia) **\+ [\<alt-text\>](#\<alt-text\>) e [\<long-desc\>](#\<long-desc\>)** em vídeo

```
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<label>Video 1</label>
 		<caption>
 			 <title>Vídeo: malesuada vehicula</title> 
 </caption> 
<alt-text>Breve descrição do objeto (até 120 caracteres)</alt-text>
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
</media>
```

[**\<media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia) **\+ [\<long-desc\>](#\<long-desc\>) e [\<xref\>](#\<xref\>:-referência-cruzada)** para transcrição de áudio em [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>)

```
<media mimetype="audio" mime-subtype="mp3" xlink:href="1234-5678-scie-58-e1043-md1.mp3">
<label>Audio 1</label>
 		<caption>
 			 <title>título</title> 
 </caption> 
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
<xref ref-type="sec" rid="TR1"/>
</media>
```

| Atenção:  Quando houver o uso combinado [\<alt-text\>](#\<alt-text\>) não pode ter conteúdo ser null, neste caso não use [\<alt-text\>](#\<alt-text\>). |
| :---- |

| Consulte no SPS:  [\<alt-text\>](#\<alt-text\>); [\<long-desc\>](#\<long-desc\>); [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>). |
| :---- |

\----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

# **🔹LISTA DE MARCAÇÃO** {#🔹lista-de-marcação}

Esta lista compreende apenas os elementos XML do Estilo [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema). A lista completa dos elementos XML que compõem a tag set da JATS na versão [1.3](https://jats.nlm.nih.gov/publishing/tag-library/1.3/) deve ser consultada se necessário.

### **Sugestão de Atribuição de @id**  {#sugestão-de-atribuição-de-@id}

Para a composição do @id, deve-se combinar o prefixo do tipo de elemento com um número inteiro, como segue:

| Atenção:  Todo @rid obrigatoriamente deve ter um @id correspondente no XML; Um @id pode ou não ter um @rid correspondente no XML; Atenção para os elementos que exigem @id/@rid. |
| :---- |

| Descrição | Elemento XML | Prefixo | Exemplo |
| ----- | ----- | :---: | ----- |
| Afiliação | [\<aff\>](#\<aff\>:-afiliação-de-autores) | **aff** | aff1, aff2, … |
| Apêndice | [\<app\>](#\<app-group\>:-apêndice-e-anexo) | **app** | app1, app2, … |
| Autor Correspondente | [\<corresp\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/corresp.html) | **c** | c1, c2, … |
| Equação e Fórmula | [\<disp-formula\>, \<inline-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada)  | **e** | e1, e2, … |
| Figura | [\<fig\>](#\<fig\>:-figura) | **f** | f1, f2, … |
| Graphic para Imagem | [\<graphic\> e \<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) | **gf** | gf1, gf2, … |
| Material Suplementar | [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar) | **suppl** | suppl1, suppl3, … |
| MathML | [\<mml:math\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/mml-math.html) | **m** | m1, m2, … |
| Nota de Documento e Autor | [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) | **fn** | fn1, fn2, … |
| Nota de Rodapé de Tabela | [\<table-wrap-foot\>](#\<table-wrap\>:-tabela) \+ [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) | **TFN** | TFN1, TFN2, … |
| Objeto Multimídia | [\<media\> e \<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) | **md** | md1, md2, … |
| Referência Bibliográfica | [\<ref\>](#\<ref-list\>:-lista-de-referências) | **B** | B1, B2, … |
| Relação entre Documentos | [\<related-article\>](#\<related-article\>:-relação-entre-documentos) | **r** | r1, r2, … |
| Resumo Visual (Visual Abstract) | [\<abstract abstract-type="graphical"\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief) | **vs** | vs1 … |
| Seção | [\<sec\>](#\<sec\>:-seção-de-texto) | **sec** | sec1, sec2, … |
| Seção de Transcrição | [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>) | **TR** | TR1, TR2, … |
| Sub Artigo / Conjunto de Respostas | [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) e [\<response\>](#\<response\>:-conjunto-de-respostas) | **S** | S1, S2, … |
| Tabela | [\<table-wrap\>](#\<table-wrap\>:-tabela) | **t** | t1, t2, … |

## **\<abstract\>: Resumo, Highlights, Visual Abstract e In Brief**  {#<abstract>:-resumo,-highlights,-visual-abstract-e-in-brief}

***resumo traduzido \<trans-abstract\> e palavras-chave \<kwd-group\>***

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Zero ou mais vezes |

Além do resumo textual simples ou estruturado (com seções [\<sec\>](#\<sec\>:-seção-de-texto)), pode-se ter os tipos a seguir, sendo mandatório o atributo para @abstract-type com os valores:

1. graphical: Resumo Visual com imagem **(Visual Abstract)** \- Imagem que representa o texto do resumo de um documento.  
2. key-points: Destaques do Documento **(Highlights)** \- Palavras que transmitem os resultados principais do documento.  
3. summary: Resumo curto **(In Brief)** \- Texto resumido e curto sobre a pesquisa.

O atributo @xml:lang se faz necessário quando o documento apresenta resumos traduzidos [\<trans-abstract\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief) diferentes do idioma original declarado em [\<article\>](#\<article\>:-artigo) ou [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo).

* Resumos [\<abstract\>]() e [\<trans-abstract\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief), simples e estruturado, **exigem** palavras-chave [\<kwd-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/kwd-group.html);  
* Resumos [\<abstract\>]() graphical, key-points e summary, **não** permitem palavras-chave [\<kwd-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/kwd-group.html).

[\<kwd-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/kwd-group.html) obrigatoriamente possui o atributo @xml:lang e exige um \<title\> seguido de um ou mais [\<kwd\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/kwd.html).

Independente de como é apresentada a ordem dos resumos no PDF, na marcação XML a ordem deve considerar:

1. \<abstract\>;   
2. \<abstract **abstract-type="graphical"**\>;  
3. \<abstract **abstract-type="key-points"**\>;  
4. \<abstract **abstract-type="summary"**\>.

**Exemplos:**

**Resumo Simples (em dois idiomas):** [\<abstract\> \+ \<trans-abstract\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief)

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="en">
...
<abstract>
 	<title>Abstract</title>
<p>Sed sollicitudin elit eu nunc elementum consectetur. Fusce sed velit eu dui rhoncus facilisis a at arcu. Ut venenatis nisl id orci tincidunt, vitae fermentum tellus aliquam. Phasellus feugiat scelerisque mi vitae aliquet. Nam nec aliquam ex, molestie convallis elit. In luctus nibh eu elit pharetra laoreet. Mauris quis tristique tortor.</p>
</abstract>
<trans-abstract xml:lang="pt">
 	<title>Resumo</title>
<p>Sed sollicitudin elit eu nunc elementum consectetur. Fusce sed velit eu dui rhoncus facilisis a at arcu. Ut venenatis nisl id orci tincidunt, vitae fermentum tellus aliquam. Phasellus feugiat scelerisque mi vitae aliquet. Nam nec aliquam ex, molestie convallis elit. In luctus nibh eu elit pharetra laoreet. Mauris quis tristique tortor.</p>
</trans-abstract>
<kwd-group xml:lang="en">
<title>Keywords:</title>
<kwd>quia</kwd>
<kwd>dolor</kwd>
<kwd>amet</kwd>
<kwd>consectetur</kwd>
<kwd>Romeu Zemaadipisci</kwd>
</kwd-group>
<kwd-group xml:lang="pt">
<title>Palavras-chave:</title>
<kwd>Neque</kwd>
<kwd>porro</kwd>
<kwd>quisquam</kwd>
<kwd>dolorem</kwd>
<kwd>ipsum</kwd>
</kwd-group>
```

**Resumo Estruturado:** [\<sec\>](#\<sec\>:-seção-de-texto)

```
<abstract>
	<title>Resumo</title>
		<sec>
			<title>Objetivo</title>
			<p>In rhoncus, felis non tempor mattis, nisl purus commodo turpis, nec volutpat leo tortor ac elit. Vestibulum et nisi elit. Maecenas gravida est ac maximus sodales.</p>
		</sec>
		<sec>
			<title>Métodos</title>
			<p>Sed sollicitudin elit eu nunc elementum consectetur. Fusce sed velit eu dui rhoncus facilisis a at arcu. Ut venenatis nisl id orci tincidunt, vitae fermentum tellus aliquam.</p>
</sec>
<sec>
			<title>Resultados</title>
			<p>Phasellus feugiat scelerisque mi vitae aliquet. Nam nec aliquam ex, molestie convallis elit. In luctus nibh eu elit pharetra laoreet. Mauris quis tristique tortor.</p>
		</sec>
		<sec>
			<title>Conclusão</title>
			<p>Morbi lobortis odio in ligula venenatis dapibus. Pellentesque tincidunt aliquet finibus. Nunc pulvinar feugiat aliquet. Proin facilisis in mauris vitae finibus. Maecenas at fermentum risus.</p>
		</sec>
</abstract>
<kwd-group xml:lang="pt">
<title>Palavras-chave:</title>
<kwd>Neque</kwd>
<kwd>porro</kwd>
<kwd>quisquam</kwd>
<kwd>dolorem</kwd>
<kwd>ipsum</kwd>
</kwd-group>
```

**Visual Abstract:** @abstract-type="graphical"

```
<abstract abstract-type="graphical">
  <title>Visual Abstract</title>
    <p>
      <fig id="vs1">
         <caption>
            <title>Título</title>
         </caption>
         <graphic xlink:href="1234-5678-scie-58-e1043-vs1.jpg"/>
      </fig>
    </p>
</abstract>
```

**Highlights:** @abstract-type="key-points"

```
<abstract abstract-type="key-points">
  <title>HIGHLIGHTS</title>
    <p>Nam vitae leo aliquet, pretium ante at, faucibus felis</p>
    <p>Aliquam ac mauris et libero pulvinar facilisis</p>
    <p>Fusce aliquam ipsum ut diam luctus porta</p>
    <p>Ut a erat ac odio placerat convallis</p>
</abstract>
```

**In Brief:** @abstract-type="summary"

```
<abstract abstract-type="summary">
  <title>In Brief</title>
    <p>Nam vitae leo aliquet, pretium ante at, faucibus felisMauris erat magna, mollis in ex id, porta venenatis risus. Nulla facilisi. Morbi mattis, lectus sit amet lacinia efficitur.</p>    
</abstract>
```

| Atenção:  Não usar [\<list\>](#\<list\>:-lista) \+ [\<list-item\>](#\<list\>:-lista) para o tipo [\<abstract abstract-type="key-points"\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief), o tipo de resumo já define o formato em lista; Os resumos [\<abstract\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief) dos tipos graphical, key-points e summary não comportam palavras-chave [\<kwd-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/kwd-group.html). |
| :---- |

| Consulte na JATS:  [\<kwd-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/kwd-group.html); [\<kwd\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/kwd.html). |
| :---- |

| Consulte no SPS:  [\<article\>: Artigo](#\<article\>:-artigo); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [Nomeação de Arquivos](#nomeação-de-arquivos); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |

## **\<aff\>: Afiliação de Autores**  {#<aff>:-afiliação-de-autores}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| \<front\> | Uma ou mais vezes |
| [\<contrib-group\>](#\<contrib-group\>:-autoria) | Uma ou mais vezes |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Uma ou mais vezes |

Atributos obrigatórios:

3. Em [\<aff\>](#\<aff\>:-afiliação-de-autores) @id  
4. Em \<country\> @country  
5. Em \<institution\> @content-type

| Valores para @content-type em \<institution\> |  Descrição |
| :---: | ----- |
| **orgname** | Representa a instituição de primeiro nível hierárquico mencionada na afiliação. |
| **orgdiv1** | Representa, hierarquicamente, a primeira divisão da instituição mencionada. |
| **orgdiv2** | Representa, hierarquicamente, a segunda divisão da instituição mencionada. |
| **original** | Identifica a afiliação completa conforme consta no texto do documento. |

A afiliação dos autores identifica a sua localização institucional e geográfica no momento em que a pesquisa foi realizada, sendo obrigatória para todos os autores na coleção SciELO Brasil. A afiliação é denominada institucional porque, em geral, a localização é uma instituição juridicamente estabelecida e relacionada com a pesquisa, mas pode ser outro tipo de instância como programa, projeto, rede, etc. No caso de autores que não possuem vínculo institucional, a afiliação deve ser identificada como Pesquisador Autônomo, incluindo os demais elementos da localização geográfica. A afiliação geográfica deve incluir a cidade, o estado e o país. Os autores podem ter mais de uma afiliação institucional.

Todos os tipos de documentos, sem exceção, devem ter autoria com especificação completa das instâncias institucionais e geográficas de localização dos autores quando a pesquisa foi realizada e o manuscrito preparado. Cada instância institucional é identificada por nomes de até três níveis hierárquicos ou programáticos e pela localização geográfica (cidade, estado e país) em que está localizada. Quando um autor é afiliado a mais de uma instância, cada afiliação deve ser identificada separadamente. Quando dois ou mais autores estão afiliados à mesma instância, a identificação da instância é feita uma única vez. 

As instâncias acadêmicas são as mais comuns de afiliação dos autores. Estruturas típicas de afiliação acadêmica combinam, normalmente, dois ou três níveis hierárquicos, como por exemplo: departamento-faculdade-universidade, programa de pós-graduação-faculdade-universidade, instituto de pesquisa-universidade, hospital-faculdade de medicina-universidade, etc. São comuns também institutos, empresas, clínicas e fundações públicas ou privadas, relacionados com pesquisa e desenvolvimento. Ocorrem também instâncias que desenvolvem ou participam de pesquisa que são órgãos de governo, ligados a ministérios, autarquias, empresas estatais, secretarias estaduais ou municipais. Há ainda os autores afiliados a instâncias programáticas ou envolvendo comunidades de pesquisadores ou profissionais que funcionam em torno de um programa, projeto ou rede e podem ter vida limitada.

A apresentação da afiliação deve guardar uniformidade em todos os documentos e recomenda-se o seguinte formato:

* A identificação do grupo de afiliações deve vir logo abaixo dos nomes dos autores. Quando diferentes autores têm diferentes afiliações os nomes e as afiliações são obrigatoriamente relacionados entre si por etiquetas \<label\>;  
* A identificação das instâncias institucionais deve, sempre que aplicável, indicar as unidades hierárquicas correspondentes. Recomenda-se que as unidades hierárquicas sejam apresentadas em ordem decrescente, por exemplo, universidade, faculdade e departamento;  
* Em nenhum caso as afiliações devem vir acompanhadas das titulações ou mini currículos dos autores. Estes, quando presentes, devem ser publicadas separadamente das afiliações como notas;  
* O endereço do autor correspondente deve ser apresentado separadamente e pode vir no final do documento;  
* Os nomes das instituições e programas deverão ser apresentados por extenso e no idioma original da instituição ou na versão em inglês, quando a escrita não é latina. Veja os exemplos:  
  * Universidade de São Paulo, Faculdade de Medicina, Departamento de Pediatria, São Paulo, SP, Brasil;  
  * Universidad Nacional Autónoma de México, Instituto de Investigaciones Biomédicas, Departamento de Pediatría, Ciudad de México, México;  
  * Johns Hopkins University, School of Medicine, Department of Pediatrics;  
* Os nomes dos autores devem obrigatoriamente vir acompanhados dos respectivos identificadores ORCID.

**Exemplos:**

**Afiliação completa:** original, orgname, orgdiv1, orgdiv2, \<city\>, \<state\>, \<country\> e \<email\>

```
<aff id="aff1">
    <label>1</label>
    <institution content-type="orgname">Fundação Oswaldo Cruz</institution>
    <institution content-type="orgdiv1">Escola Nacional de Saúde Pública Sérgio Arouca</institution>
    <institution content-type="orgdiv2">Centro de Estudos da Saúde do Trabalhador e Ecologia Humana</institution>
    <addr-line>
        <city>Manguinhos</city>
        <state>RJ</state>
    </addr-line>
    <country country="BR">Brasil</country>
    <email>denise.peres@email.com</email>
    <institution content-type="original">Fundação Oswaldo Cruz; da Escola Nacional de Saúde Pública Sérgio Arouca, do Centro de Estudos da Saúde do Trabalhador e Ecologia Humana. RJ - Manguinhos, Brasil. denise.peres@email.com</institution>
</aff>
```

**Afiliação para Pesquisador Autônomo**

```
<aff id="aff1">
 	<label>1</label>
<country country="BR">Brasil</country>
<institution content-type="original">Pesquisador Autônomo, Brasil.</institution>
</aff>
```

| Atenção:  Divisões abaixo do terceiro nível hierárquico da instituição são identificadas somente no elemento \<institution content-type="original"\>; Para autores autônomos também é mandatório a informação de país nas afiliações; Para o valor de @country preencher de acordo com a [Norma ISO 3166](https://www.iso.org/obp/ui/#search), com os códigos de dois caracteres alfabéticos em caixa alta; Email de autores devem ser marcados com a tag \<email\> em [\<aff\>](#\<aff\>:-afiliação-de-autores) ou [\<corresp\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/corresp.html) se email de autor correspondente. Quando existente em [\<aff\>](#\<aff\>:-afiliação-de-autores) deve estar presente em \<institution content-type="original"\>; Mini currículo ou biográfica de autores devem ser marcados em [\<bio\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/bio.html); Titulação de autores, quando informado junto ao nome do autor sem informações adicionais biográficas ([\<bio\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/bio.html)), deve ser marcada em [\<degrees\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/degrees.html). |
| :---- |

| Consulte:   [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.8.4. Afiliação institucional dos(as) autores(as)*;  [Norma ISO 3166](https://www.iso.org/obp/ui/#search). |
| :---- |

| Consulte na JATS:   [\<bio\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/bio.html); [\<corresp\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/corresp.html); [\<degrees\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/degrees.html). |
| :---- |

| Consulte no SPS:  [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis); [\<fn\>: Nota de Autor, Documento e Tabela](#\<fn\>:-nota-de-autor,-documento-e-tabela); [\<contrib-group\>: Autoria](#\<contrib-group\>:-autoria); [\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |

## **\<alternatives\>: .svg** {#<alternatives>:-.svg}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<table-wrap\>](#\<table-wrap\>:-tabela) | Zero ou mais vezes |
| [\<disp-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) | Zero ou mais vezes |
| [\<inline-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) | Zero ou mais vezes |

Elemento usado opcionalmente para armazenar um grupo de alternativas em imagem [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura), para processamento de um determinado conjunto informacional em versões logicamente equivalentes (substituto). O elemento só poderá ser utilizado em dados que estão originalmente codificados tais como tabela ou equação e sua imagem equivalente. 

* Ao renderizar um objeto fornecido em [\<alternatives\>](#\<alternatives\>:-.svg), a opção mais acessível deve ser usada, por isso só serão aceitas imagens no formato SVG (Scalable Vector Graphics) que é um formato de imagem baseado em XML que usa vetores, ou seja, equações matemáticas para descrever formas, linhas e cores. Como a imagem é uma alternativa para um conteúdo XML codificado e portanto acessível, as imagens em .svg em [\<alternatives\>](#\<alternatives\>:-.svg) não devem possuir a marcação dos elementos [\<alt-text\>](#\<alt-text\>) e [\<long-desc\>](#\<long-desc\>).

| Atenção:  Em [\<alternatives\>](#\<alternatives\>:-.svg) as imagens em [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) devem, obrigatoriamente, possuir o formato SVG. |
| :---- |

| Consulte no SPS:  [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [Nomeação de Arquivos](#nomeação-de-arquivos). |
| :---- |


**Exemplos:**

**Imagem alternativa .svg** em [tabela](#\<table-wrap\>:-tabela)

```
<table-wrap id="t5">
    <label>Tabela 5</label>
    <caption>
        <title>Alíquota menor para prestadores</title>
    </caption>
    <alternatives>
        <graphic xlink:href="1234-5678-scie-58-e1043-gf1.svg"/>
        <table>
            <thead>
                <tr>
                    <th rowspan="3">Proposta de Novas Tabelas - 2016</th>
                </tr>
                <tr>
                    <th>Receita Bruta em 12 Meses - em R$</th>
                    <th>Anexo I - Comércio</th>
                    <th>Anexo II Indústria</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>De R$ 225.000,01 a RS 450.000,00</td>
                    <td>4,00%</td>
                    <td>4,50%</td>
                </tr>
                <tr>
                    <td>De R$ 450.000,01 a R$ 900.000,00</td>
                    <td>8,25%</td>
                    <td>8,00%</td>
                </tr>
                <tr>
                    <td>De R$ 900.000,01 a R$ 1.800.000,00</td>
                    <td>11,25%</td>
                    <td>12,25%</td>
                </tr>
            </tbody>
        </table>
    </alternatives>
    <table-wrap-foot>
        <fn id="TFN1">
            <p>A informação de alíquota do anexo II é significativa</p>
        </fn>
    </table-wrap-foot>
</table-wrap>
```

**Imagem alternativa .svg** em [fórmula](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada)

```
<inline-formula id="e3">
    <alternatives>
        <mml:math id="m3">
            <mml:mrow>
                <mml:msup>
                    <mml:mover accent="true">
                        <mml:mi>σ</mm:mi>
                        <mml:mo>ˆ</fl:mo>
                    </mml:mover>
                    <mml:mn>2</mml:mn>
                </mml:msup>
            </mml:mrow>
        </mml:math>
        <graphic xlink:href="1234-5678-scie-58-e1043-gf3.svg"/>
    </alternatives>
</inline-formula>
```

## **\<app-group\>: Apêndice e Anexo** {#<app-group>:-apêndice-e-anexo}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| \<back\> | Zero ou uma vez |

Atributo obrigatório para [\<app\>](#\<app-group\>:-apêndice-e-anexo) :

1. @id

Apêndice ou anexo de um documento são elementos pós-textuais e representam:

| Tipo | Descrição |
| :---: | ----- |
|  **Apêndice**  | Consiste em um texto ou documento **elaborado pelo autor**, com o objetivo de complementar a sua argumentação, sem prejuízo algum da unidade nuclear do trabalho. É um elemento opcional. |
| **Anexo** | Consiste em texto ou documento **não elaborado pelo autor**, que irá servir de fundamentação, comprovação ou ilustração. É um elemento opcional. |

Tanto o apêndice quanto o anexo devem estar integralmente incluídos no documento original (PDF).

Devem ser marcados em \<back\> e [\<app\>](#\<app-group\>:-apêndice-e-anexo) exigem o elemento \<label\> como título do apêndice ou anexo e opcionalmente pode-se usar \<caption\> \+ \<title\>. A terminologia usada para Apêndice, Anexo e Material Suplementar depende das instruções aos autores de cada periódico.

O elemento [\<app-group\>](#\<app-group\>:-apêndice-e-anexo) deve sempre ser usado como agrupador do elemento [\<app\>](#\<app-group\>:-apêndice-e-anexo), mesmo se houver somente uma ocorrência deste último.

* Para acessibilidade recomenda-se que  vídeos e áudios venham com sua descrição em [\<alt-text\>](#\<alt-text\>) e/ou [\<long-desc\>](#\<long-desc\>) mais a transcrição do conteúdo na seção [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>).

| Atenção:  [\<app-group\>](#\<app-group\>:-apêndice-e-anexo) e [\<app\>](#\<app-group\>:-apêndice-e-anexo) não comportam [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar). |
| :---- |

| Consulte no SPS:  [MARCAÇÃO PARA ACESSIBILIDADE](#🔹marcação-para-acessibilidade) [\<alt-text\>](#\<alt-text\>); [\<long-desc\>](#\<long-desc\>); [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<ext-link\>](#\<ext-link\>:-link). |
| :---- |

**Exemplo:** 

```
<app-group>
  <title>Appendix</title>
    <app id="app1">
        <label>Appendix 1</label>		
		<title>Questionnaire for student inclusion</title>		       
        	<graphic xlink:href="1234-5678-scie-58-e1043-app1.jpg"/>
    </app>
   <app id="app2">
<label>Appendix 2</label>
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<label>Video 1</label>
 		<caption>
 			 <title>Video 1</title> 
 </caption>
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
<xref ref-type="sec" rid="TR1"/>
</media>
    </app>   
    <app id="app3">
        <label>Appendix 3</label>
        <p>Pellentesque sollicitudin, purus nec ultricies tristique, purus nisi imperdiet enim, nec mollis augue odio sit amet augue. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut cursus ipsum non nisi faucibus suscipit. Cras ut venenatis tellus.</p>
    </app>
  	<app id="app4">
        <label>Appendix 4</label>
        <p>Para mais informações <ext-link ext-link-type="uri" xlink:href="http://www.scielo.org">clique aqui</ext-link> para verificar o pdf.</p>
    </app>
  	<app id="app5">
        <label>Appendix 5</label>
        <table-wrap>
            <caption>
                <title>Título da tabela</title>
            </caption>
            <table frame="hsides" rules="all">
                <colgroup width="XX%">
                    <col/>
                    <col/>
                    <col/>
                </colgroup>
                <thead>
                    <tr>
                        <th style="background-color:#e5e5e5">xxxxx</th>
                        <th style="background-color:#e5e5e5">xxxxx</th>
                        <th style="background-color:#e5e5e5">xxxxxx</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td align="center">xxxxx</td>
                        <td align="center">xxxx</td>
                        <td align="center">xxxx</td>
                    </tr>
                </tbody>
            </table>
        </table-wrap>
    </app>
</app-group>
```

## **\<article\>: Artigo**  {#<article>:-artigo}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| / | Uma vez |

[\<article\>](#\<article\>:-artigo) é a raiz do XML do documento e deve explicitar, obrigatoriamente, os atributos de versão da DTD, tipo de documento, idioma do texto, declarações de namespace e versão do [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema) utilizada.

Atributos obrigatórios:

1. @dtd-version="1.3"  
2. @article-type  
3. @xml:lang  
4. @xmlns:mml  
5. @xmlns:xlink="http://www.w3.org/1999/xlink"  
6. @specific-use="sps-1.10"

O atributo @xmlns:mml="http://www.w3.org/1998/Math/MathML" deve ser utilizado sempre que equações do tipo MathML forem identificadas no documento.

Para @dtd-version deve-se utilizar o valor 1.3 conforme a DTD, explicitada em \<\!DOCTYPE\>.

O idioma do texto em @xml:lang é descrito pela [norma ISO 639-1](https://pt.wikipedia.org/wiki/ISO_639#:~:text=ISO%20639%20%C3%A9%20formado%20por,l%C3%ADngua%20\(idiomas\)%20do%20planeta.) como um código de dois caracteres alfabéticos em caixa baixa.

O atributo @specific-use identifica a versão utilizada da SciELO Publishing Schema, sps-1.10. 

Para o atributo @article-type os valores possíveis são:

| Consulte no SPS:  [DOCUMENTOS INDEXÁVEIS E NÃO INDEXÁVEIS](#🔹documentos-indexáveis-e-não-indexáveis):  [Documentos Indexáveis](#heading=h.7lpqbk8y8ugb); [Documentos Não Indexáveis](#documentos-não-indexáveis); [Equivalência entre documentos indexáveis e @article-type em \<article\>](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>). |
| :---- |

| Valor | Descrição |
| :---: | ----- |
| **addendum** | Um trabalho publicado que agrega informação ou esclarecimento a outro trabalho (é diferente do tipo "errata" que corrige um erro em um material publicado previamente). Consulte: [Adendo](#adendo) e [Guia para Publicação de Adendo](https://wp.scielo.org/wp-content/uploads/guia_adendo.pdf). |
| **article-commentary** | Um documento cujo objeto ou foco é(são) outro(s) documento(s); documento que comenta outros documentos. Este tipo de documento pode ser usado quando o(a) editor(a) de uma publicação convida um(a) autor(a) com uma opinião oposta para comentar um documento controverso e então publica os dois documentos juntos. O tipo "editorial" que tem similaridade é reservado para comentários escritos pelo(a) editor(a) ou membro da equipe editorial ou autor(a) convidado(a). Documento, posição ou pensamento coletivo elaborado em conjunto com pesquisadores(as) experts em determinados assuntos. Consulte: [Comentário](#comentário). |
| **book-review** | Resenha ou análise crítica de um ou mais livros impressos ou online. Consulte: [product\>: Resenha de Livro](#\<product\>:-resenha-de-livro). |
| **brief-report** | Comunicação sucinta de resultados de pesquisa. |
| **case-report** | Estudo de caso, relato de caso, ou outra descrição de um caso. |
| **clinical-instruction** | Documento de um guia ou diretriz estabelecida por uma autoridade biomédica ou de outra área como um comitê, sociedade, ou agência do governo. Consulte: [Ensaio Clínico](#ensaio-clínico). |
| **correction** | Modificação ou correção de material publicado previamente. Em inglês é chamado também de "*correction*". (O tipo "adendo" aplica-se apenas para material adicionado a um material publicado previamente). Consulte: [Errata](#errata) e [Guia para Publicação de Errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf). |
| **data-article** | Documento que descreve dados de pesquisa no texto do documento ou disponibilizados em um repositório de dados. |
| **discussion** | Discussão convidada relacionado com um documento específico ou um número do periódico. |
| **editorial** | Peça de opinião, declaração política ou comentário geral escrito por membro da equipe editorial (com autoria e título próprio diferente do título da seção). |
| **letter** | Carta dirigida ao periódico, tipicamente comentando um trabalho publicado. Consulte: [Carta](#carta). |
| **expression-of-concern** | Documento publicado pelo periódico para alertar os leitores sobre possíveis problemas em um documento previamente publicado, como indícios de má conduta ou erros graves ainda em investigação. A manifestação não confirma as suspeitas, mas informa a comunidade científica até a conclusão da apuração. Consulte: [Manifestação de Preocupação](#manifestação-de-preocupação) e [Guia para publicação de Manifestação de Preocupação](https://wp.scielo.org/wp-content/uploads/guia_manifestacao.pdf). |
| **obituary** | Anúncio do falecimento ou elogio a um(a) colega falecido(a) recentemente,  com análise da obra e da contribuição do autor homenageado com aporte de conteúdo científico. |
| **oration** | Discurso, documento de uma fala ou apresentação oral. |
| **other** | Quando o documento tem conteúdo científico que justifica sua indexação mas nenhum dos tipos anteriores se aplica.  Entrevista: Ato de entrevistar ou ser entrevistado(a). É uma conversa entre duas ou mais pessoas com um fim determinado com perguntas feitas pelo(a) entrevistador(a) de modo a obter informação necessária por parte do(a) entrevistado(a). Expediente Anual (*Ad Hoc*): Documento de publicação anual que apresenta a composição completa do corpo editorial, incluindo editores de seção e conselho editorial, bem como a lista de pareceristas ad hoc que colaboraram com o processo de revisão por pares ao longo do ano. Este documento visa a registrar e agradecer a contribuição de todos os envolvidos na publicação do periódico, reforçando o compromisso com a transparência editorial. |
| **partial-retraction** | Retratação ou negação de parte ou partes de material publicado previamente. Consulte: [Retratação](#retratação) e [Guia para Publicação de Retratação](https://wp.scielo.org/wp-content/uploads/guia_retratacao.pdf). |
| **rapid-communication** | Atualização de uma pesquisa ou outros itens noticiosos. |
| **reply** | Resposta a uma carta ou comentário, tipicamente pelo(a) autor(a)original comentando sobre comentários. Consulte: [XML da Resposta para uma Carta](#xml-da-resposta-para-uma-carta), [XML da Resposta para um Comentário](#xml-da-resposta-para-um-comentário) e [\<response\>: Conjunto de Respostas](#\<response\>:-conjunto-de-respostas). |
| **research-article** | Artigo que comunica uma pesquisa original. |
| **retraction** | Retratação ou negação de um material publicado previamente. Consulte: [Retratação](#retratação) e [Guia para Publicação de Retratação](https://wp.scielo.org/wp-content/uploads/guia_retratacao.pdf). |
| **review-article** | Artigo que sumariza criticamente o conhecimento científico sobre um determinado tema. Também conhecido como revisão de literatura. Ensaio, reflexão circunstanciada, com maior liberdade por parte do(a) autor(a) para defender determinada posição, que vise a aprofundar a discussão ou que apresente nova contribuição/abordagem a respeito de tema relevante. Métodos, documento que descreve avanços metodológicos, incluindo métodos inovadores e aprimoramento de métodos existentes. O documento deve incluir evidências da eficácia do método e comparações com os métodos anteriormente disponíveis. |
| **reviewer-report** | Parecer de documento aprovado, documento de análise de um manuscrito que comunica pesquisa com avaliação da sua relevância, dos métodos aplicados e apresentação e discussão dos resultados obtidos. O parecer destaca as contribuições da pesquisa que recomendam sua aceitação e as recomendações de correções e aperfeiçoamentos. Consulte: [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta). |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="en">
...
</article>
```

| Consulte no SPS:  [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [Adendo](#adendo); [Carta](#carta); [Comentário](#comentário); [Ensaio Clínico](#ensaio-clínico); [Errata](#errata); [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta); [Retratação](#retratação); [\<response\>: Conjunto de Respostas](#\<response\>:-conjunto-de-respostas); [\<product\>: Resenha de Livro](#\<product\>:-resenha-de-livro). |
| :---- |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf): *5.2.3. Tipos de documentos*; [norma ISO 639-1](https://pt.wikipedia.org/wiki/ISO_639#:~:text=ISO%20639%20%C3%A9%20formado%20por,l%C3%ADngua%20\(idiomas\)%20do%20planeta.); [Guia para Publicação de Errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf); [Guia para Publicação de Retratação](https://wp.scielo.org/wp-content/uploads/guia_retratacao.pdf); [Guia para Publicação de Adendo](https://wp.scielo.org/wp-content/uploads/guia_adendo.pdf); [Guia para publicação de Manifestação de Preocupação](https://wp.scielo.org/wp-content/uploads/guia_manifestacao.pdf). |
| :---- |

## 

## **\<article-categories\>: Seção de Documento** {#<article-categories>:-seção-de-documento}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Uma vez |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Uma vez |

Atributo obrigatório:

1. Em [\<subj-group\>](#\<article-categories\>:-seção-de-documento) @subj-group-type="heading"

Designa a seção a qual pertence o documento, a mesma indicada no PDF, e é um dado obrigatório para qualquer tipo de documento. Usualmente é utilizada para classificar documentos por assunto. É obrigatória a presença de somente uma ocorrência do elemento [\<subj-group\>](#\<article-categories\>:-seção-de-documento) com o atributo @subj-group-type="heading". 

O idioma da seção deve corresponder ao mesmo idioma declarado no @xml:lang de [\<article\>](#\<article\>:-artigo) e [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo).

**Exemplos:**

```
<article-categories>
    <subj-group subj-group-type="heading">
        <subject>Original Article</subject>
    </subj-group>
</article-categories>
```

```
<article-categories>
    <subj-group subj-group-type="heading">
        <subject>Estudos Literários e Culturais: Artigo</subject>
    </subj-group>
</article-categories>
```

```
<article-categories>
    <subj-group subj-group-type="heading">
        <subject>Dossiê PSICOLOGIA SOCIAL E ANTIRRACISMO: compromisso social e político por um outro Brasil</subject>
    </subj-group>
</article-categories>
```

```
<article-categories>
    <subj-group subj-group-type="heading">
        <subject>Scientific Communication: Food Safety</subject>
    </subj-group>
</article-categories>
```

Seção de documento declarada no [PDF](https://www.scielo.br/j/aabc/a/9mt8YP8JQFn6HCTF6FFtfVp/?format=pdf&lang=en):

![Exemplo da parte superior da primeira página de um PDF de artigo da revista Anais da Academia Brasileira de Ciências publicado em SciELO, indicando no canto superior esquerdo a seção do documento intitulada BIOMEDICAL SCIENCES escrito em preto.O PDF tem fundo branco, letras pretas e cinzas e parte do logo da revista do canto superior esquerdo. O logo representa o perfil de um(a) guerreiro(a) com capacete e armadura e em frente de seu rosto aparece a escrita NÁA.][image4]

Seção de documento marcada no [XML](https://www.scielo.br/j/aabc/a/43MZPpyk7MBx8ZnFjMpyFCQ/?format=xml):

![Exemplo da marcação XML da seção BIOMEDICAL SCIENCES no artigo da revista Anais da Academia Brasileira de Ciências publicado em SciELO.A seção é marcada dentro da estrutura XML com os elementos: \<article-categories\>, \<subj-group subj-group-type="heading"\> e \<subject\>.][image5]

Apresentação da seção na **página do artigo** no [site SciELO](https://www.scielo.br/j/aabc/a/43MZPpyk7MBx8ZnFjMpyFCQ/?lang=en):

![Exemplo da apresentação da seção BIOMEDICAL SCIENCES na página do artigo em SciELO da revista Anais da Academia Brasileira de Ciências.A seção é o primeiro item da página do artigo no canto esquerdo escrito em cinza seguido dos dados bibliográfico do artigo: título abreviado, volume, número, ano e link DOI clivável.][image6]

Apresentação da seção no **sumário online** do [site SciELO](https://www.scielo.br/j/aabc/i/2025.v97n1/):

![Exemplo da apresentação da seção BIOMEDICAL SCIENCES no sumário do eletrônico da revista Anais da Academia Brasileira de Ciências.A seção escrita em branco fica acima do título do artigo no canto esquerdo dentro um círculo oval verde.][image7]

| Atenção:  Seções que tenham indicação de subseção, por exemplo Scientific Communication: Food Safety, devem ser identificadas na mesma tag de [\<subject\>](#\<article-categories\>:-seção-de-documento) separada por dois pontos, traço, etc.; O idioma da seção, tanto no PDF quanto no XML, deve corresponder ao idioma do documento. |
| :---- |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*:* *5.2.8.1. Textos em XML – SciELO Publishing Schema;* [Guia para Implementação da Modalidade de Publicação Contínua em Periódicos Científicos](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf); [Guia para a Gestão e Criação de Números Other para Ordenação de Seções de Periódicos que Adotam a Modalidade de Publicação Contínua (PC)](https://docs.google.com/document/d/1HvLyoMxBF0WWTbwnDLYGbQmf9Z6OkrL-x8EWaGmRnbg/edit?tab=t.0). |
| :---- |

| Consulte no SPS:  [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<article\>: Artigo](#\<article\>:-artigo); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

## **\<article-id\>: DOI e Other**  {#<article-id>:-doi-e-other}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Uma ou mais vezes |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Uma ou mais vezes |

Atributos obrigatórios:

1. @pub-id-type="doi"  
2. @pub-id-type="other"

Identificador único do documento em uma base de dados. Sendo mandatório a identificação de DOI para qualquer coleção, onde documentos multilíngues devem ter DOIs distintos para cada versão de idioma na coleção SciELO Brasil. A criação do número DOI é de responsabilidade dos periódicos e o CrossRef só aceita a [criação do sufixo DOI](https://wp.scielo.org/wp-content/uploads/orientacao_doi.pdf) com o seguinte parâmetro: "**a-z**", "**A-Z**", "**0-9**" e "**\-.\_;()/**", ou seja, utilize letras maiúsculas ou minúsculas (o CrossRef não faz diferenciação entre elas) sem acentuação, números de 0 a 9 sempre no formato arábico (0, 1, 2, 3, 4, 5, 6, 7, 8 e 9\) e os caracteres: hífen, ponto final, underline, ponto e vírgula, parênteses e barra (não use barra invertida). Outros caracteres podem impedir a ativação do DOI junto ao CrossRef. 

O Other é uma numeração sequencial que contém 5 dígitos, obrigatório para periódicos que adotam a modalidade contínua (PC) e que dá a ordenação das seções dentro do sumário online do periódico na página do SciELO. As planilhas de ordenação de seção (other) são criadas pela SciELO e geridas juntamente com os prestadores de serviços XML contratados pelos periódicos ou suas equipes editoriais, quando estes produzem seus XMLs.

| Valor  | Descrição |
| :---: | ----- |
| **doi** | DOI \- Digital Object Identifier. |
| **other** | Utilizado para ordenar documentos na modalidade de Publicação Contínua (PC). |

| Atenção:  A tag [\<article-id pub-id-type="doi"\>](#\<article-id\>:-doi-e-other) deve ser repetida em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) somente se o DOI for diferente do documento no idioma original. |
| :---- |

| Consulte:  [Comunicado](https://mailchi.mp/scielo/doi-traducoes) DOIs para tradução enviado em 23/03/2022; [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*:* *5.2.9.2. Indexação dos metadados no Crossref* e *5.2.5. Multilinguismo – texto completo e metadados*;  [Guia para Implementação da Modalidade de Publicação Contínua em Periódicos Científicos](https://wp.scielo.org/wp-content/uploads/guia_pc.pdf);  [Guia para a Gestão e Criação de Números Other para Ordenação de Seções de Periódicos que Adotam a Modalidade de Publicação Contínua (PC)](https://docs.google.com/document/d/1HvLyoMxBF0WWTbwnDLYGbQmf9Z6OkrL-x8EWaGmRnbg/edit?tab=t.0); [Orientação para criação do DOI](https://wp.scielo.org/wp-content/uploads/orientacao_doi.pdf); [Diretrizes para exibição de DOIs do Crossref](https://wp.scielo.org/wp-content/uploads/Diretriz_DOI_PT.pdf); [Guia do usuário do Digital Object Identifier](https://www.abecbrasil.org.br/arquivos/Guia_usuario_DOI-online3.pdf). |
| :---- |

**Exemplo:**

```
<article-meta>
		<article-id pub-id-type="doi">10.1590/S2237-96222023000200017</article-id>
		<article-id pub-id-type="other">00603</article-id>
```

#### **Outros usos para número other** {#outros-usos-para-número-other}

O [\<article-id pub-id-type="other"\>](#\<article-id\>:-doi-e-other) além de ser um elemento obrigatório para periódicos que publicam em modalidade de publicação contínua PC que publicam com paginação digital (elocation-id), também deve obrigatoriamente ser usado para revistas que publicam em modalidade regular nos seguintes casos: 

1. Um ou mais documentos do fascículo com paginação, não arábica como, por exemplo, número romano (*artigo 1:* pág I até IV / *artigo 2:* pág 1 até 26 e etc)   
2. Documentos sem paginação sequencial iniciada em 1 a cada documento (*artigo 1:* pág 1 até 15 / *artigo 2:* pág 1 até 26 e etc)   
3. Documentos com paginação sobrepostas (*artigo 1*: pág 1 até **15** / *artigo 2*: pág **14** até 26 e etc **ou** *artigo 1*: pág 1 até **15** / *artigo 2:* pág **15** até 26 e etc)

Nestes casos, mesmo que o problema ocorra em apenas um documento, todo o conjunto de documentos do fascículo deve ter a adição do [\<article-id pub-id-type="other"\>](#\<article-id\>:-doi-e-other) com número other obrigatoriamente com 5 dígitos, sempre iniciando em 00001 em diante, respeitando a ordem dos documentos no sumário impresso (ou a ordem designada pela equipe editorial do periódico). Para estes casos a planilha de other não é necessária, uma vez que os números são entregues fechados, sem a adição posterior de documentos no número.

***Exemplo:*** Revistas em modalidade regular, cujo fascículo possui um editorial com paginação romana I até IV, seguido de 5 documentos com paginação arábica sequencial. O fascículo, que contém seis documentos, deverá ser entregue pelo prestador XML considerando:

* **Editorial:** \<article-id pub-id-type="other"\>**00001**\</article-id\>  
* **Artigo 1:** \<article-id pub-id-type="other"\>**00002**\</article-id\>   
* **Artigo 2:** \<article-id pub-id-type="other"\>**00003**\</article-id\>   
* **Artigo 3:** \<article-id pub-id-type="other"\>**00004**\</article-id\>   
* **Artigo 4:** \<article-id pub-id-type="other"\>**00005**\</article-id\>   
* **Artigo 5:** \<article-id pub-id-type="other"\>**00006**\</article-id\>

## **\<contrib-group\>: Autoria**  {#<contrib-group>:-autoria}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Uma ou mais vezes |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Uma ou mais vezes |

Contém o grupo de elementos relativos à contribuição na elaboração do documento. Os contribuintes mais frequentes são os autores pessoais, instituições e grupos de pesquisa. A indicação de autoria é mandatória para publicação na coleção SciELO Brasil, exceto para errata, retratação, adendo e Manifestação de Preocupação .

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*:* *5.2.8.2. Autoria – identificação, afiliação institucional e contribuição.* |
| :---- |

| Consulte no SPS:  [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis); [\<contrib\>: \<name\> e \<collab\>](#\<contrib\>:-\<name\>-e-\<collab\>); [\<contrib-id\>: ORCID](#\<contrib-id\>:-orcid); [\<role\>: Papel do Autor \- Taxonomia CRediT](#\<role\>:-papel-do-autor---taxonomia-credit). |
| :---- |

### 

### **\<contrib\>: \<name\> e \<collab\>** {#<contrib>:-<name>-e-<collab>}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| [\<contrib\>](#\<contrib\>:-\<name\>-e-\<collab\>) Aparece em | Ocorre |
| :---: | :---: |
| [\<contrib-group\>](#\<contrib-group\>:-autoria) | Uma ou mais vezes |

Atributo obrigatório:

1. @contrib-type

Identifica dados individuais, institucionais ou de grupo, de contribuintes do documento, tais como: [\<name\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/name.html) e [\<collab\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/collab.html), podendo ser inclusive anônimos [\<anonymous\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/anonymous.html). Os dados dos autores em [\<contrib-group\>](#\<contrib-group\>:-autoria) obrigatoriamente devem estar relacionados com uma [\<xref\>](#\<xref\>:-referência-cruzada) com a afiliação correspondente em [\<aff\>](#\<aff\>:-afiliação-de-autores), exemplo: \<xref ref-type="aff" rid="aff1"\>1\</xref\>. Quando não há menção de etiqueta usa-se a tag fechada \<xref ref-type="aff" rid="aff1"/\>.

* O atributo @contrib-type define o tipo de contribuição e pode ter os valores (exceto para parecer, o valor **author** é mandatório):

| Valor  | Descrição |
| :---: | ----- |
| **author** | Autor do conteúdo. |
| **compiler** | Compilador \- Indivíduo que compilou o conteúdo a partir de várias fontes. |

Outros tipos de contribuidores como tradutor, ilustrador, assistente de pesquisa, etc., devem ser identificados em [\<author-notes\>](#\<author-notes\>-+-\<fn\>:-notas-de-autor), com \<fn\> e @fn-type="other". Para informar [o(s) editor(es) responsável(is) pelo processo de avaliação](#declaração-de-editor-responsável-pelo-processo-de-avaliação) usa-se @fn-type="edited-by".

| Consulte na JATS:  [\<anonymous\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/anonymous.html); [\<bio\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/bio.html); [\<collab\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/collab.html); [\<degrees\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/degrees.html); [\<name\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/name.html); [\<suffix\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/suffix.html). |
| :---- |

| Consulte no SPS:  [Declaração de Editor Responsável pelo Processo de Avaliação](#declaração-de-editor-responsável-pelo-processo-de-avaliação); [\<aff\>: Afiliação de Autores](#\<aff\>:-afiliação-de-autores); [\<fn\>: Nota de Autor, Documento e Tabela](#\<fn\>:-nota-de-autor,-documento-e-tabela); [\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada); [\<contrib-id\>: ORCID](#\<contrib-id\>:-orcid); [\<role\>: Papel do Autor \- Taxonomia CRediT](#\<role\>:-papel-do-autor---taxonomia-credit). |
| :---- |

**Exemplos:**

**Autores pessoas físicas**

```
<contrib-group>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0001-8528-2091</contrib-id>
        <contrib-id contrib-id-type="scopus">24771926600</contrib-id>
        <name>
            <surname>Einstein</surname>
            <given-names>Albert</given-names>
        </name>  
      <xref ref-type="aff" rid="aff1">1</xref>
 <role>conception</role>
 		 <role>design</role>
            <role>methodology</role>
            <role>wrote the first draft</role>
            <role>revision</role>
            <role>approved</role>
    </contrib>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0002-4134-5932</contrib-id>
        <contrib-id contrib-id-type="lattes">4760273612238540</contrib-id>
        <name>
            <surname>Meneghini</surname>
            <given-names>Rogerio</given-names>
        </name>  
      <xref ref-type="aff" rid="aff2">2</xref>
 <role>wrote the first draft</role>
            <role>revision</role>
            <role>approved</role>
    </contrib>   
</contrib-group>
```

**Autores pessoas físicas com sufixo [\<suffix\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/suffix.html) em sobrenome** 

```
<contrib-group>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0002-4134-5932</contrib-id>        
        <name>
            <surname>Meneghini</surname>
            <given-names>Rogerio</given-names>
		 <suffix>Junior</suffix>
        </name>  
      <xref ref-type="aff" rid="aff1">1</xref>
    </contrib>   
     <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0001-8528-2091</contrib-id> 
        <name>
            <surname>Einstein</surname>
            <given-names>Albert</given-names>
 <suffix>Neto</suffix>
        </name>  
      <xref ref-type="aff" rid="aff2">2</xref>
    </contrib>
</contrib-group>
```

| Atenção:  Não condicione a marcação da tag \<suffix\> ao aparecimento dos termos Filho, Júnior, Neto, Netto ou Sobrinho nos nomes completos de autores; Alguns termos podem representar primeiro(s) nome(s) de autore(s), exemplo: Junior da Silva, Renato Junior da Silva, Neto Gonçalves. Estes dados só serão considerados sufixo de sobrenome quando representarem grau de parentesco, sempre identificados ao final; Sufixo relacionado a grau de parentesco é exclusivamente utilizado para nomes masculinos segundo o Código de Catalogação Anglo-Americano (AACR2), ou seja, Sobrinha, Neta e Filha são sobrenomes que não cabem na marcação de \<suffix\>. |
| :---- |

**Autores pessoas físicas com sobrenome composto** 

```
<contrib-group>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0002-4134-5932</contrib-id>        
        <name>
            <surname>RABELO-PONTE</surname>
            <given-names>Antônio Diego</given-names>		
        </name>  
      <xref ref-type="aff" rid="aff1">1</xref>
    </contrib>   
     <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0001-8528-2091</contrib-id> 
        <name>
            <surname>DE MARTINO</surname>
            <given-names>José Márcio</given-names>
        </name>  
      <xref ref-type="aff" rid="aff2">2</xref>
    </contrib>
</contrib-group>
```

| Atenção:  Em nomes de origem hispânica é comum encontrar sobrenomes interligados com hífen, que representam sobrenomes compostos. Nestes casos, mesmo que o sobrenome contenha Junior ou Neto, mantenha o dado como sobrenome. exemplo: \<surname\>Meneghini-Junior\</surname\>; Outras formatações para representar nomes compostos podem ser utilizadas na diagramação do PDF, tais como: caixa alta e negrito; A marcação de sobrenome composto só deverá ocorrer quando houver identificação clara no campo de autoria do documento de que o autor possui um sobrenome composto. Por isso, é essencial que as revistas definam um padrão na instrução aos autores; Sobrenomes compostos que foram publicados incorretamente, pois não tinham identificação clara no PDF do documento original no campo autoria, só serão corrigidos mediante a publicação de uma [errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf). |
| :---- |

| Consulte:  [Guia para Publicação de Errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf)*: 4.2 Correções em sobrenome composto de autores*. |
| :---- |

**Autores pessoas físicas com informação de título acadêmico [\<degrees\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/degrees.html)**

```
<contrib-group>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0002-4134-5932</contrib-id>        
        <name>
            <surname>Meneghini</surname>
            <given-names>Rogerio</given-names>
        </name>  
 <degrees>PhD</degrees>  
      <xref ref-type="aff" rid="aff2">2</xref>
 <role>wrote the first draft</role>
            <role>revision</role>
            <role>approved</role>
    </contrib>   
</contrib-group>
```

**Autores pertencentes a um grupo**

```
<contrib-group>
   <contrib contrib-type="author" id="collab">
     <collab>The SciELO Group</collab>
     <xref ref-type="author-notes" rid="fn1">1</xref>
   </contrib>
</contrib-group>
<contrib-group content-type="collab-list">	
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0001-0002-0003</contrib-id>
      <name>
           <surname>Esteves</surname>
           <given-names>Felipe</given-names>
      </name>
      <xref ref-type="aff" rid="aff1">1</xref>
    </contrib>
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0004-0005-0006</contrib-id>
      <name>
           <surname>Souza</surname>
           <given-names>Joyce Miranda</given-names>
      </name>
      <xref ref-type="aff" rid="aff2">2</xref>
    </contrib>
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0007-0008-0009</contrib-id>
      <name>
           <surname>Torres</surname>
           <given-names>Karen</given-names>
      </name>
      <xref ref-type="aff" rid="aff1">1</xref>
    </contrib>
</contrib-group>
```

**Autores pessoas físicas \+ pertencentes a grupo**

```
<contrib-group>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0001-8528-2091</contrib-id>
        <name>
            <surname>Einstein</surname>
            <given-names>Albert</given-names>
        </name>  
      <xref ref-type="aff" rid="aff1">1</xref>
<role>conception</role>
 		<role>design</role>
            <role>methodology</role>
            <role>wrote the first draft</role>
            <role>revision</role>
            <role>approved</role>
    </contrib>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0002-4134-5932</contrib-id>
        <name>
            <surname>Meneghini</surname>
            <given-names>Rogerio</given-names>
        </name>  
      <xref ref-type="aff" rid="aff2">2</xref>
 <role>wrote the first draft</role>
            <role>revision</role>
            <role>approved</role>
    </contrib>   
</contrib-group>
<contrib-group>
   <contrib contrib-type="author" id="collab">
     <collab>The SciELO Group</collab>
     <xref ref-type="author-notes" rid="fn1">1</xref>
   </contrib>
</contrib-group>
<contrib-group content-type="collab-list">	
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0001-0002-0003</contrib-id>
      <name>
           <surname>Esteves</surname>
           <given-names>Felipe</given-names>
      </name>
      <xref ref-type="aff" rid="aff3">3</xref>
    </contrib>
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0004-0005-0006</contrib-id>
      <name>
           <surname>Souza</surname>
           <given-names>Joyce Miranda</given-names>
      </name>
      <xref ref-type="aff" rid="aff4">4</xref>
    </contrib>
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0007-0008-0009</contrib-id>
      <name>
           <surname>Torres</surname>
           <given-names>Karen</given-names>
      </name>
      <xref ref-type="aff" rid="aff5">5</xref>
    </contrib>
</contrib-group>
```

| Atenção:  É obrigatória a identificação dos nomes dos autores pertencentes ao grupo em \<contrib-group content-type="collab-list"\> (e descritos no PDF do documento); Os autores devem possuir também suas afiliações completas (e descritas no PDF do documento); Os autores devem informar seus ORCIDs (e descritos no PDF do documento); Sem a identificação dos nomes que fazem parte do grupo, estes autores não conseguirão atribuir o DOI do documento como um trabalho de sua autoria nas bases de dados curriculares; Se o documento possuir um [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) com @article-type="translation", lembre-se de alterar o valor do @id e @rid, para não coincidir com os mesmos dados de \<[article\>](#\<article\>:-artigo) exemplo: Se em \<[article\>](#\<article\>:-artigo): \<contrib contrib-type="author" id\="collab"\>; \<contrib contrib-type="author" rid\="collab"\>... Em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) deve ser: \<contrib contrib-type="author" id\="collab1"\>; \<contrib contrib-type="author" rid\="collab1"\>... |
| :---- |

### **\<contrib-id\>: ORCID** {#<contrib-id>:-orcid}

***e outros identificadores digitais de pesquisador***

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<contrib\>](#\<contrib\>:-\<name\>-e-\<collab\>) | Zero ou mais vezes |

Atributo obrigatório:

* @contrib-id-type="orcid"

O atributo @contrib-id-type possui os seguintes valores, sendo o valor orcid obrigatório para a coleção SciELO Brasil:

| Valor  | Descrição |
| :---: | ----- |
| **lattes** | Identifica um pesquisador no Currículo Lattes. |
| **orcid** | Identifica um pesquisador na ORCID Organization. |
| **researchid** | Identifica um pesquisador no sistema da Clarivate. |
| **scopus** | Identifica um pesquisador no sistema da Scopus. |

| Atenção:  Para o conteúdo de [\<contrib-id\>](#\<contrib-id\>:-orcid) não use links (https://…), utilize apenas os dados alfanuméricos relativos ao identificador; Autores pertencentes a um grupo obrigatoriamente devem ter a identificação de ORCID  (e descritos no PDF do documento). |
| :---- |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.8.3. Identificação ORCID iD;*  [ORCID Brand Guidelines](https://info.orcid.org/brand-guidelines/). |
| :---- |

| Consulte no SPS:  [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis); [\<contrib\>: \<name\> e \<collab\>](#\<contrib\>:-\<name\>-e-\<collab\>); [\<role\>: Papel do Autor \- Taxonomia CRediT](#\<role\>:-papel-do-autor---taxonomia-credit). |
| :---- |

**Exemplos:**

**Autores pessoas físicas**

```
<contrib-group>
    <contrib contrib-type="author">
        <contrib-id contrib-id-type="orcid">0000-0001-8528-2091</contrib-id>
        <contrib-id contrib-id-type="scopus">24771926600</contrib-id>
   <contrib-id contrib-id-type="lattes">4760273612238540</contrib-id>
        <name>
            <surname>Einstein</surname>
            <given-names>Albert</given-names>
        </name>  
      <xref ref-type="aff" rid="aff1">1</xref>
    </contrib>      
</contrib-group>
```

**Autores pertencentes a grupo**

```
<contrib-group>
   <contrib contrib-type="author" id="collab">
     <collab>The SciELO Group</collab>
     <xref ref-type="author-notes" rid="fn1">1</xref>
   </contrib>
</contrib-group>
<contrib-group content-type="collab-list">	
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0001-0002-0003</contrib-id>
      <name>
           <surname>Esteves</surname>
           <given-names>Felipe</given-names>
      </name>
      <xref ref-type="aff" rid="aff1">1</xref>
    </contrib>
    <contrib contrib-type="author" rid="collab">
    <contrib-id contrib-id-type="orcid">0000-0004-0005-0006</contrib-id>
      <name>
           <surname>Souza</surname>
           <given-names>Joyce Miranda</given-names>
      </name>
      <xref ref-type="aff" rid="aff2">2</xref>
    </contrib>      
</contrib-group>
```

### 

### **\<role\>: Papel do Autor \- Taxonomia CRediT**  {#<role>:-papel-do-autor---taxonomia-credit}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<contrib-group\>](#\<contrib-group\>:-autoria) | Zero ou mais vezes |

Os papéis dos autores devem ser marcados no XML em [\<role\>](#\<role\>:-papel-do-autor---taxonomia-credit). O periódico e/ou os autores podem usar qualquer taxonomia. SciELO Brasil recomenda o uso da taxonomia [CRediT](https://credit.niso.org/). Para o uso de [CRediT](https://credit.niso.org/), adiciona-se em [\<role\>](#\<role\>:-papel-do-autor---taxonomia-credit) um @content-type com a URL do papel descrito no [CRediT](https://credit.niso.org/).

Para o preenchimento do @content-type [CRediT](https://credit.niso.org/), usar:

| Valor | URL CRediT |
| :---: | ----- |
| **Conceptualization** | [https://credit.niso.org/contributor-roles/conceptualization/](https://credit.niso.org/contributor-roles/conceptualization/) |
| **Data curation** | [https://credit.niso.org/contributor-roles/data-curation/](https://credit.niso.org/contributor-roles/data-curation/) |
| **Formal analysis** | [https://credit.niso.org/contributor-roles/formal-analysis/](https://credit.niso.org/contributor-roles/formal-analysis/) |
| **Funding acquisition** | [https://credit.niso.org/contributor-roles/funding-acquisition/](https://credit.niso.org/contributor-roles/funding-acquisition/) |
| **Investigation** | [https://credit.niso.org/contributor-roles/investigation/](https://credit.niso.org/contributor-roles/investigation/) |
| **Methodology** | [https://credit.niso.org/contributor-roles/methodology/](https://credit.niso.org/contributor-roles/methodology/) |
| **Project administration** | [https://credit.niso.org/contributor-roles/project-administration/](https://credit.niso.org/contributor-roles/project-administration/) |
| **Resources** | [https://credit.niso.org/contributor-roles/resources/](https://credit.niso.org/contributor-roles/resources/) |
| **Software** | [https://credit.niso.org/contributor-roles/software/](https://credit.niso.org/contributor-roles/software/) |
| **Supervision** | [https://credit.niso.org/contributor-roles/supervision/](https://credit.niso.org/contributor-roles/supervision/) |
| **Validation** | [https://credit.niso.org/contributor-roles/validation/](https://credit.niso.org/contributor-roles/validation/) |
| **Visualization** | [https://credit.niso.org/contributor-roles/visualization/](https://credit.niso.org/contributor-roles/visualization/) |
| **Writing – original draft** | [https://credit.niso.org/contributor-roles/writing-original-draft/](https://credit.niso.org/contributor-roles/writing-original-draft/) |
| **Writing – review e editing** | [https://credit.niso.org/contributor-roles/writing-review-editing/](https://credit.niso.org/contributor-roles/writing-review-editing/) |

**Exemplos:**

**Papéis sem taxonomia definida ou outras taxonomias que não sejam a [CRediT](https://credit.niso.org/).**

```
<contrib-group>
<contrib contrib-type="author">
	<contrib-id contrib-id-type="orcid">1234-0002-3629-9732</contrib-id>
 		<name>
 			<surname>Santos</surname>
 			<given-names>Anderson</given-names>
		</name>
 		<xref ref-type="aff" rid="aff1">1</xref>
 		<role>conception</role>
 		<role>design</role>
           <role>methodology</role>
           <role>wrote the first draft</role>
           <role>revision</role>
           <role>approved</role>
 	</contrib>
 <contrib-group>
```

**Papéis com a taxonomia [CRediT](https://credit.niso.org/)**

```
<contrib-group>
	<contrib contrib-type="author">
 	<contrib-id contrib-id-type="orcid">1234-0001-9486-8465</contrib-id>
 		<name>
 			<surname>Rosa</surname>
 			<given-names>Nathália</given-names>
 		</name>
 	<xref ref-type="aff" rid="aff1">1</xref>
 	<role content-type="http://credit.niso.org/contributor-roles/conceptualization/">Conceptualization</role>
 	<role content-type="http://credit.niso.org/contributor-roles/data-curation/">Data curation</role>
 	<role content-type="http://credit.niso.org/contributor-roles/formal-analysis/">Formal analysis</role>
 	<role content-type="http://credit.niso.org/contributor-roles/writing-original-draft/">Writing – original draft</role>
 	<role content-type="http://credit.niso.org/contributor-roles/validation/">Validation</role>
 </contrib>
```

Os [pareceres](#parecer:-revisão-por-pares-aberta) marcados como [\<article\>](#\<article\>:-artigo) ou [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo), obrigatoriamente também carregam a marcação de [\<role\>](#\<role\>:-papel-do-autor---taxonomia-credit) em [\<contrib-group\>](#\<contrib-group\>:-autoria) com o atributo obrigatório @specific-use onde os valores possíveis são:

| Valor | Descrição |
| :---: | ----- |
| **reviewer** | Revisor/Parecerista |
| **editor** | Editor |

**Exemplo:**

```
<contrib-group>
    <contrib contrib-type="author">
 	  <contrib-id contrib-id-type="orcid">0000-0001-0002-0003</contrib-id>               
      <name>
              <surname>Silva</surname>
              <given-names>Marcos</given-names>
            </name>
            <role specific-use="reviewer">Parecerista</role>
            <xref ref-type="aff" rid="aff1"/>
     </contrib>
</contrib-group>          
```

| Atenção:  Cada papel de autor deve ser marcado em uma tag de [\<role\>](#\<role\>:-papel-do-autor---taxonomia-credit), esta tag não deve ser usada para marcação do conjunto de vários papéis; Se o periódico ou os autores não usarem a taxonomia [CRediT](https://credit.niso.org/), não use o @content-type, mesmo que alguns papéis sejam idênticos ou similares aos termos [CRediT](https://credit.niso.org/). Ou o documento adota os termos [CRediT](https://credit.niso.org/) na íntegra ou não adota; O uso de taxonomia é relacionado ao documento e não necessariamente ao lote ou revista; A tradução dos termos para outros idiomas fica a critério do periódico e autores que desejarem utilizá-los; Para os papéis dos autores não use a tag [\<fn fn-type="con"\>](#\<author-notes\>-+-\<fn\>:-notas-de-autor). |
| :---- |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.6.4.1. Créditos às autoras e autores*; [Contributor Role Taxonomy (CRediT)](https://credit.niso.org/). |
| :---- |

| Consulte no SPS:  [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta); [\<article\>: Artigo](#\<article\>:-artigo); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [\<author-notes\> \+ \<fn\>: Notas de Autor](#\<author-notes\>-+-\<fn\>:-notas-de-autor); [\<contrib\>: \<name\> e \<collab\>](#\<contrib\>:-\<name\>-e-\<collab\>); [\<contrib-id\>: ORCID](#\<contrib-id\>:-orcid). |
| :---- |

## **\<disp-formula\> e \<inline-formula\>: Equação e Fórmula Codificada**  {#<disp-formula>-e-<inline-formula>:-equação-e-fórmula-codificada}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

Estes elementos identificam equações e fórmulas exibidas em bloco ou em um parágrafo, sendo: 

1. [\<disp-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada): Identifica equações e fórmulas exibidas em bloco, fora de um parágrafo;  
2. [\<inline-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada): Identifica equações e fórmulas exibidas em um parágrafo.

Atributo obrigatório:

1. Em [\<disp-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada): @id  
2. Em [\<inline-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada): @id  
3. Em [\<mml:math\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/mml-math.html): @id

Para equações e fórmulas, a codificação pode ser escrita de acordo com [W3C](https://www.w3.org/about/) em linguagem [MathML](http://www.w3.org/TR/MathML3/), sendo o elemento base [\<mml:math\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/mml-math.html), ou com codificação TeX ou LaTeX.

* Para acessibilidade recomenda-se a codificação sempre em [MathML](http://www.w3.org/TR/MathML3/), que permite que serviços de apresentação forneçam os recursos necessários para tornar as equações e fórmulas acessíveis.


| Atenção:  Fórmulas e equações obrigatoriamente devem ser codificadas, preferencialmente em [MathML](http://www.w3.org/TR/MathML3/); Para compor os @id, use o prefixo “e” para [\<disp-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) e [\<inline-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) e ”m” para [\<mml:math\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/mml-math.html); Fórmulas e equações [\<disp-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) devem aparecer no XML logo abaixo da primeira chamada do texto, independente de onde o dado esteja no PDF, no entanto, apenas quando identificadas fora de [\<app-group\>](#\<app-group\>:-apêndice-e-anexo) e [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar). |
| :---- |

| Consulte:  [Comunicado](http://us4.campaign-archive2.com/?u=f26dcf71797dd37381acb4aa5&id=0211ed957f&e=%5BUNIQID) codificação de fórmulas enviado em 09/12/2016; [Mathematical Markup Language (MathML) Version 3.0 2nd Edition](https://www.w3.org/TR/MathML3/); [W3C](https://www.w3.org/about/). |
| :---- |


| Consulte na JATS:  [\<mml:math\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/mml-math.html); [\<tex-math\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/tex-math.html). |
| :---- |

| Consulte no SPS:  [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |


**Exemplos:**

[**\<inline-formula\>**](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) **usando** [MathML](http://www.w3.org/TR/MathML3/)

```
<p>Nulla velit magna, semper quis dignissim id, condimentum nec diam
<inline-formula id="e3">
    <mml:math id="m3">
        <mml:mrow>
            <mml:msup>
                <mml:mover accent="true">
                    <mml:mi>σ</mml:mi>
                    <mml:mo>ˆ</mml:mo>
                </mml:mover>
                <mml:mn>2</mml:mn>
            </mml:msup>
        </mml:mrow>
    </mml:math>
</inline-formula>
Nulla quis leo sed turpis congue finibus feugiat ut dui. Donec id tincidunt tellus. Nunc fermentum dolor et congue convallis.<p/>
```

**Duas [\<inline-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) no mesmo parágrafo mais uma [\<disp-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) usando** [MathML](http://www.w3.org/TR/MathML3/)

```
<p>The displacement vector 
			<inline-formula id="e7">
					<mml:math display="inline" id="m7" overflow="scroll">
						<mml:mrow>
							<mml:msub>
								<mml:mi>U</mml:mi>
								<mml:mi>W</mml:mi>
							</mml:msub>
						</mml:mrow>
					</mml:math>
			</inline-formula> of the water layer can be expressed using the potential function 
			<inline-formula id="e8">
					<mml:math display="inline" id="m8" overflow="scroll">
						<mml:mrow>
							<mml:msub>
								<mml:mi>φ</mml:mi>
								<mml:mi>W</mml:mi>
							</mml:msub>
						</mml:mrow>
					</mml:math>
			</inline-formula>, as follows:</p>
			<disp-formula id="e9">			
				<mml:math display="block" id="m9" overflow="scroll">
					<mml:mrow>
						<mml:msub>
							<mml:mi>U</mml:mi>
							<mml:mi>W</mml:mi>
						</mml:msub>
						<mml:mo>=</mml:mo>
						<mml:mo>∇</mml:mo>
						<mml:msub>
							<mml:mi>φ</mml:mi>
							<mml:mi>W</mml:mi>
						</mml:msub>
					</mml:mrow>
				</mml:math>
<label>(3)</label>				
			</disp-formula>
```

[**\<disp-formula\>**](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) **usando** [MathML](http://www.w3.org/TR/MathML3/)

```
<disp-formula id="e2">
<label>(2)</label>
		<mml:math display="block" id="m2" overflow="scroll">
			<mml:mrow>
				<mml:msub>
					<mml:mi>K</mml:mi>
					<mml:mi>W</mml:mi>
				</mml:msub>
				<mml:mo>∇</mml:mo>
				<mml:mo>∇</mml:mo>
				<mml:mo>⋅</mml:mo>
				<mml:msub>
					<mml:mi>U</mml:mi>
					<mml:mi>W</mml:mi>
				</mml:msub>
				<mml:mo>=</mml:mo>
				<mml:msub>
					<mml:mi>ρ</mml:mi>
					<mml:mi>W</mml:mi>
				</mml:msub>
				<mml:msub>
					<mml:mover accent="true">
						<mml:mi>U</mml:mi>
						<mml:mo>¨</mml:mo>
					</mml:mover>
					<mml:mi>W</mml:mi>
				</mml:msub>
			</mml:mrow>
		</mml:math>				
</disp-formula>
```

[**\<disp-formula\>**](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) **usando LaTeX**

```
<disp-formula id="e10">
    <label>(1)</label>
    <tex-math id="tx1">
        \documentclass{article}
        \usepackage{wasysym}
        \usepackage[substack]{amsmath}
        \usepackage{amsfonts}
        \usepackage{amssymb}
        \usepackage{ambassy}
        \usepackage[mathscr]{eucal}
        \usepackage{mathrsfs}
        \usepackage{pmc}
        \usepackage[Euler]{upgreek}
        \pagestyle{empty}
        \oddsidemargin -1.0in
        \begin{document}
        \[E_it=α_i+Z_it γ+W_it δ+C_it θ+∑_i^n EFind_i+∑_t^n EFtemp_t+ ε_it \]
        \end{document}
    </tex-math>
</disp-formula>
```

## **\<ext-link\>: link**  {#<ext-link>:-link}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<aff\>](#\<aff\>:-afiliação-de-autores) | Zero ou mais vezes |
| \<article-meta\> | Zero ou mais vezes |
| [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) | Zero ou mais vezes |
| [\<attrib\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/attrib.html) | Zero ou mais vezes |
| [\<copyright-statement\>](#\<permissions\>:-licença-creative-commons-e-copyright) | Zero ou mais vezes |
| [\<corresp\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/corresp.html) | Zero ou mais vezes |
| [\<disp-formula\>](#\<disp-formula\>-e-\<inline-formula\>:-equação-e-fórmula-codificada) | Zero ou mais vezes |
| [\<element-citation\>](#\<ref-list\>:-lista-de-referências) | Zero ou uma vez |
| [\<fig\>](#\<fig\>:-figura) | Zero ou mais vezes |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Zero ou mais vezes |
| [\<funding-statement\>](#\<funding-group\>:-financiamento-e-apoio) | Zero ou mais vezes |
| [\<mixed-citation\>](#\<ref-list\>:-lista-de-referências) | Zero ou uma vez |
| \<p\> | Zero ou mais vezes |
| [\<product\>](#\<product\>:-resenha-de-livro) | Zero ou mais vezes |
| [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar) | Zero ou mais vezes |
| [\<table-wrap\>](#\<table-wrap\>:-tabela) | Zero ou mais vezes |
| [\<trans-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) | Zero ou mais vezes |

[\<ext-link\>](#\<ext-link\>:-link) é usado para marcar link ou hyperlink text, exceto quando [\<related-article\>](#\<related-article\>:-relação-entre-documentos) for requerido ou quando houver identificadores digitais de pesquisador [\<contrib-id\>](#\<contrib-id\>:-orcid) em [\<contrib-group\>](#\<contrib-group\>:-autoria). Não é permitido o uso dos elementos \<uri\> ou \<self-uri\>.

* Para acessibilidade, o melhor formato para representar links do tipo uri no corpo do texto é usar hyperlink text e não apenas o link (exceto quando o link compor dados de [referência bibliográfica](#\<ref-list\>:-lista-de-referências)). O texto envolvido por [\<ext-link\>](#\<ext-link\>:-link) deve ser conciso e deve descrever significativamente o objeto vinculado. Por exemplo:   
  * evite o uso de:   
    * **\<ext-link\>**[Leia mais](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema)**\</ext-link\>** sobre link no SPS.  
  * use:   
    * **\<ext-link\>**[Leia mais sobre link no SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema)**\</ext-link\>**.   
* Quando o hyperlink text ou link não possuir descrição suficiente sobre o objeto, opcionalmente pode-se usar o atributo [@xlink:title](https://jats.nlm.nih.gov/publishing/tag-library/1.3/attribute/xlink-title.html), contendo apenas texto, números ou caracteres especiais.

Atributos obrigatórios:

1. @ext-link-type  
2. @xlink:href

Os valores possíveis para @ext-link-type podem ser observados na [JATS](https://jats.nlm.nih.gov/publishing/tag-library/1.3/attribute/ext-link-type.html), com a adição do valor clinical-trial, os mais utilizados são:

| Valor | Descrição |
| :---: | ----- |
| **clinical-trial** | Link para registro de [ensaio clínico](#ensaio-clínico)  |
| **uri** | Link para website |
| **doi** | Link DOI precedido de doi.org/ ou dx.doi.org/ |
| **pmcid** | Link para o documento no [PubMed Central](https://pmc.ncbi.nlm.nih.gov/) \- contém na url o identificador pmcid |
| **pmid** | Link para o documento no [PubMed](https://pubmed.ncbi.nlm.nih.gov/) \- contém na url o identificador pmid |

**Exemplos:**

[**\<ext-link\>**](#\<ext-link\>:-link): link uri

```
<ext-link ext-link-type="uri" xlink:href="https://www.scielo.br/">www.scielo.br/</ext-link>
```

[**\<ext-link\>**](#\<ext-link\>:-link): hyperlink text uri

```
<ext-link ext-link-type="uri" xlink:href="https://www.scielo.br/">SciELO Brasil</ext-link>
```

[**\<ext-link\>**](#\<ext-link\>:-link): hyperlink text uri com atributo [@xlink:title](https://jats.nlm.nih.gov/publishing/tag-library/1.3/attribute/xlink-title.html)

```
<ext-link ext-link-type="uri" xlink:href="https://docs.google.com/document/d/1GTv4Inc2LS_AXY-ToHT3HmO66UT0VAHWJNOIqzBNSgA/edit?tab=t.0#heading=h.n2z5yrri2aba" xlink:title="Descrição da marcação de links externos usando SciELO Publishing Schema">Leia mais sobre link no SPS</ext-link>
```

[**\<ext-link\>**](#\<ext-link\>:-link): link uri com atributo [@xlink:title](https://jats.nlm.nih.gov/publishing/tag-library/1.3/attribute/xlink-title.html)

```
<ext-link ext-link-type="uri" xlink:href="https://docs.google.com/document/d/1GTv4Inc2LS_AXY-ToHT3HmO66UT0VAHWJNOIqzBNSgA/edit?tab=t.0" xlink:title="Guia de uso de elementos e atributos XML para documentos que seguem a implementação SciELO Publishing Schema">https://docs.google.com/document/d/1GTv4Inc2LS_AXY-ToHT3HmO66UT0VAHWJNOIqzBNSgA/edit?tab=t.0</ext-link>
```

[**\<ext-link\>**](#\<ext-link\>:-link): hyperlink text doi

```
<ext-link ext-link-type="doi" xlink:href="https://doi.org/10.1590/1806-9584-2025v33n1103677">https://doi.org/10.1590/1806-9584-2025v33n1103677</ext-link>
```

[**\<ext-link\>**](#\<ext-link\>:-link): hyperlink text pmcid

```
<ext-link ext-link-type="pmcid" xlink:href="https://pmc.ncbi.nlm.nih.gov/articles/PMC11774145/">https://pmc.ncbi.nlm.nih.gov/articles/PMC11774145/</ext-link>
```

[**\<ext-link\>**](#\<ext-link\>:-link): hyperlink text pmid

```
<ext-link ext-link-type="pmid" xlink:href="https://pubmed.ncbi.nlm.nih.gov/39879479/">https://pubmed.ncbi.nlm.nih.gov/39879479/</ext-link>
```

| Atenção:  Em @xlink:href o link deve ser inserido completo, desde o http… |
| :---- |

| Consulte na JATS:  [xlink:title](https://jats.nlm.nih.gov/publishing/tag-library/1.3/attribute/xlink-title.html). |
| :---- |

| Consulte no SPS:  [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [Ensaio Clínico](#ensaio-clínico); [\<contrib-id\>: ORCID](#\<contrib-id\>:-orcid). |
| :---- |

## **\<fig\>: Figura**  {#<fig>:-figura}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<app\>](#\<app-group\>:-apêndice-e-anexo) | Zero ou mais vezes |
| \<body\> | Zero ou mais vezes |
| [\<fig-group\>](#\<fig\>:-figura) | Zero ou mais vezes |
| [\<glossary\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/glossary.html) | Zero ou mais vezes |
| \<p\> | Zero ou mais vezes |
| [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar) | Zero ou mais vezes |

Atributo obrigatório:

3. @id

Identifica a(s) figura(s) de um documento. Nesse elemento é possível especificar \<label\>, \<caption\> \+ \<title\>, [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) e [\<attrib\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/attrib.html). Figuras com legenda traduzida devem ser marcadas em \<fig-group\> e obrigatoriamente devem trazer o atributo @xml:lang em [\<fig\>](#\<fig\>:-figura).

O elemento [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) é utilizado para identificar os tipos de imagem e tem como atributo @xlink:href que é utilizado para especificar o nome completo da imagem referenciada. Os arquivos de figura e imagem só podem conter as seguintes extensões:

* **jpg/jpeg** *(preferencialmente use esta extensão)*;  
* **png;**   
* **tif/tiff;**   
* **svg** \- apenas em [\<alternatives\>](#\<alternatives\>:-.svg).

O atributo @fig-type: é utilizado para especificar o tipo de imagem, contudo o tipo só será definido caso o \<label\> apresente um conteúdo diferente de fig, figure, figura e podem ser:

| Valor  | Descrição |
| :---: | ----- |
| **graphic** | gráfico |
| **chart** | quadro |
| **diagram** | diagrama |
| **drawing** | desenho |
| **illustration** | ilustração |
| **map** | mapa |

* Para acessibilidade recomenda-se que todas as figuras e imagens venham com sua descrição em [\<alt-text\>](#\<alt-text\>) e/ou [\<long-desc\>](#\<long-desc\>).


| Atenção:  Figuras devem aparecer no XML logo abaixo da primeira chamada do texto, independente de onde o dado esteja no PDF, no entanto, apenas quando identificadas fora de [\<app-group\>](#\<app-group\>:-apêndice-e-anexo) e [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar); Para marcação de outros objetos multimídia diferentes de figuras e imagens estáticas use [\<media\> e \<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia); |
| :---- |


| Consulte na JATS:  [\<attrib\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/attrib.html). |
| :---- |

| Consulte no SPS:  [MARCAÇÃO PARA ACESSIBILIDADE](#🔹marcação-para-acessibilidade) [\<alt-text\>](#\<alt-text\>); [\<long-desc\>](#\<long-desc\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [\<alternatives\>: .svg](#\<alternatives\>:-.svg); [Nomeação de Arquivos](#nomeação-de-arquivos); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |


**Exemplos:**

[**\<fig\>**](#\<fig\>:-figura) como mapa

```
<fig fig-type="map" id="f1">
    <label>Map 1</label>
    <caption>
        <title>Título do Mapa</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg"/>
</fig>
```

[**\<fig\>**](#\<fig\>:-figura) como figura e fonte [\<attrib\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/attrib.html)

```
<fig id="f2">
    <label>Figure 2</label>
    <caption>
        <title>Título da figura</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf2.jpg"/>
    <attrib>Fonte: IBGE (2018)</attrib>
</fig>
```

[**\<fig\>**](#\<fig\>:-figura) **(\<fig-group\>)** com legenda traduzida

```
<fig-group id="f1">
	<fig xml:lang="pt">
		<label>Figura 1:</label>
			<caption>
				<title>Representação de um elemento sensor capacitivo</title>
			</caption>
	</fig>
	<fig xml:lang="en">
		<label>Figure 1:</label>
			<caption>
				<title>Representation of a capacitive sensor element</title>
			</caption>
			<graphic xlink:href="1678-4553-ce-70-eOHYE6908-gf1.jpg"/> 
	</fig>
</fig-group>
```

[**\<fig\>**](#\<fig\>:-figura) com descrição [\<alt-text\>](#\<alt-text\>)

```
<fig id="f1">
    <label>Figura 1</label>
    <caption>
        <title>Título da Figura</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg">
    <alt-text>Breve descrição do objeto (até 120 caracteres)</alt-text>
    </graphic>
</fig>
```

## **\<fn\>: Nota de Autor, Documento e Tabela**  {#<fn>:-nota-de-autor,-documento-e-tabela}

| Aparece em | Ocorre |
| :---: | :---: |
| [\<author-notes\>](#\<author-notes\>-+-\<fn\>:-notas-de-autor) | Zero ou mais vezes |
| [\<fn-group\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento) | Zero ou mais vezes |
| [\<table-wrap-foot\>](#\<table-wrap\>:-tabela) | Zero ou mais vezes |

Atributo obrigatório para [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) em [\<author-notes\>](#\<author-notes\>-+-\<fn\>:-notas-de-autor) e [\<fn-group\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento):

1. @fn-type

Notas [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) podem conter o atributo @id e se faz necessário quando há um @rid correspondente.

Elemento usado para marcação de notas que podem ser referentes aos autores, ao documento em si ou a nota de uma tabela: 

* Notas de autor são marcadas em \<front\> dentro de [\<author-notes\>](#\<fn\>:-nota-de-autor,-documento-e-tabela);  
* Notas de documento são marcadas em \<back\> dentro de [\<fn-group\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento);  
* Notas de tabela são marcadas em [\<table-wrap-foot\>](#\<table-wrap\>:-tabela).

Notas com indicação de etiqueta (1, 2, a, b, \*, título, Nome, etc.) devem ser marcadas com o elemento \<label\>.

| Atenção:  Não use \<title\>, \<p\>, \<bold\> ou \<italic\> para identificar ou representar rótulos das notas [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) de autor, documento ou tabela; use \<label\>; O grupo de notas representadas pelos elementos [\<fn-group\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento) e [\<author-notes\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) deve ocorrer uma única vez no documento. |
| :---- |

| Consulte no SPS:  [\<author-notes\> \+ \<fn\>: Notas de Autor](#\<author-notes\>-+-\<fn\>:-notas-de-autor); [\<fn-group\> \+ \<fn\>: Notas de Documento](#\<fn-group\>-+-\<fn\>:-notas-de-documento); [\<table-wrap-foot\> \+ \<fn\>: Notas de Tabela](#\<table-wrap-foot\>-+-\<fn\>:-notas-de-tabela); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |

### **\<author-notes\> \+ \<fn\>: Notas de Autor**  {#<author-notes>-+-<fn>:-notas-de-autor}

Os valores possíveis para @fn-type são:

| Valor | Descrição |
| :---: | ----- |
| **abbr** | Representa abreviaturas de nomes dos autores. |
| **con** | Informação de contribuição de autores diferente da marcada com taxonomia CRediT ou outras taxonomias, neste último caso use apenas [\<role\>](#\<role\>:-papel-do-autor---taxonomia-credit). |
| **coi-statement** | Declaração de conflito de interesse ***(não use conflict)***. |
| **corresp** | Informações do autor correspondente. Recomenda-se o uso desta informação em [\<corresp\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/corresp.html) ao invés de [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela). |
| **current-aff** | Afiliação atual do autor. Só usar este tipo de nota quando a afiliação ao qual o autor estava afiliado quando escreveu o documento for diferente da afiliação do momento da publicação do documento. Para afiliação no geral use [\<aff\>](#\<aff\>:-afiliação-de-autores). |
| **deceased** | Pessoa falecida desde que o documento foi escrito. |
| **edited-by** | Editor do documento. (consulte [Declaração de Editor Responsável pelo Processo de Avaliação](#declaração-de-editor-responsável-pelo-processo-de-avaliação)) |
| **equal** | Informação de contribuição igualitária. |
| **on-leave** | O autor está ausente (sabático ou outro). |
| **participating-researchers** | O autor foi um pesquisador para o documento. |
| **previously-at** | Afiliação anterior do autor. |
| **study-group-members** | O autor foi um membro do grupo de estudos para a pesquisa. |
| **present-address** | Endereço atual do autor. Recomenda-se o uso destas informações em [\<corresp\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/corresp.html), caso o endereço for referente ao autor correspondente. |
| **presented-by** | Informação de trabalho apresentado pelo autor. |

Para outros tipos de notas de autor não listadas na tabela acima, deve-se usar o valor @fn-type="other".

**Exemplo:**

```
<author-notes>
    <corresp id="c1">
        <label>Correspondência</label>
      Dr. Edmundo Figueira Departamento de Fisioterapia, Universidade FISP - Hogwarts,  Brasil. E-mail: <email>contato@foo.com</email>.
    </corresp>
    <fn fn-type="coi-statement">
      <label>Conflito de Interesses</label>
        <p>Não há conflito de interesse entre os autores do artigo.</p>
    </fn>
    <fn fn-type="equal">
        <p>Todos os autores tiveram contribuição igualitária na criação do artigo.</p>
    </fn>
</author-notes>
```

| Atenção:  Não use \<title\>, \<p\>, \<bold\> ou \<italic\> para identificar ou representar rótulos das notas [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) de autor, documento ou tabela; use \<label\>. O grupo de notas representado pelo elemento [\<author-notes\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) deve ocorrer uma única vez no documento. |
| :---- |

| Consulte na JATS:  [\<corresp\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/corresp.html). |
| :---- |

| Consulte no SPS:  [\<fn\>: Nota de Autor, Documento e Tabela](#\<fn\>:-nota-de-autor,-documento-e-tabela); [\<aff\>: Afiliação de Autores](#\<aff\>:-afiliação-de-autores); [Declaração de editor responsável pelo processo de avaliação](#declaração-de-editor-responsável-pelo-processo-de-avaliação); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |

### **\<fn-group\> \+ \<fn\>: Notas de Documento**  {#<fn-group>-+-<fn>:-notas-de-documento}

Os valores possíveis para @fn-type são:

| Valor | Descrição |
| :---: | ----- |
| **abbr** | Representa abreviaturas de termos e nomes próprios utilizados ao longo do texto. |
| **com** | Representa nota de algum tipo de comunicado relevante para a realização do documento. |
| **data-availability** | Declaração de Disponibilidade de Dados (Ver mais em [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados)) |
| **financial-disclosure** | Declaração de financiamento ou negação de recursos recebidos em apoio à pesquisa na qual um documento é baseado. Presta-se para informações de financiamento que possuem um número de contrato ou que só informam que não houve financiamento. (Consulte: [\<funding-group\>: Financiamento e Apoio](#\<funding-group\>:-financiamento-e-apoio)) |
| **supported-by** | Indica que a pesquisa sobre a qual o documento é baseado foi apoiada por alguma entidade, instituição ou pessoa física. Consideram-se neste tipo, informações de financiamento ou apoio que não possuem número de contrato. (Consulte: [\<funding-group\>: Financiamento e Apoio](#\<funding-group\>:-financiamento-e-apoio)) |
| **presented-at** | Indica que o documento foi apresentado em algum evento científico. |
| **supplementary-material** | Indica o [material suplementar](#\<supplementary-material\>:-material-suplementar) do documento. Todo o material suplementar adicional deve ser informado na seção @sec-type="supplementary-material". (Veja mais informações em [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar) e [\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto))  |

Para outros tipos de notas de documento não listadas na tabela acima, deve-se usar o valor @fn-type="other".

**Exemplo:**

```
<fn-group>
    <fn fn-type="financial-disclosure" id="fn1">
        <label>Financiamento</label>
        <p>Este artigo teve financiamento da FAPESP, número de contrato #12345678</p>
    </fn>
    <fn fn-type="presented-at" id="fn2">
        <label>**</label>
        <p>Artigo foi apresentado na XVIII Conferência Internacional de Biblioteconomia 2025</p>
    </fn>
</fn-group>
```

| Atenção:  Não use \<title\>, \<p\>, \<bold\> ou \<italic\> para identificar ou representar rótulos das notas [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) de autor, documento ou tabela; use \<label\>. O grupo de nota representado pelo elemento [\<fn-group\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento) deve ocorrer uma única vez no documento. |
| :---- |

| Consulte no SPS:  [\<fn\>: Nota de Autor, Documento e Tabela](#\<fn\>:-nota-de-autor,-documento-e-tabela); [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados); [\<funding-group\>: Financiamento e Apoio](#\<funding-group\>:-financiamento-e-apoio); [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |

### **\<table-wrap-foot\> \+ \<fn\>: Notas de Tabela** {#<table-wrap-foot>-+-<fn>:-notas-de-tabela}

Notas de tabela aparecem sempre dentro de [\<table-wrap-foot\>](#\<table-wrap\>:-tabela) com uma [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) e podem conter o atributo @id.

**Exemplo:**

```
<table-wrap-foot>
        <fn id="TFN1">
            <label>*</label>
            <p>Vivamus a facilisis libero. Donec a placerat velit, sit amet rutrum nisl.</p>
        </fn>
 </table-wrap-foot>
```

| Consulte no SPS:  [\<fn\>: Nota de Autor, Documento e Tabela](#\<fn\>:-nota-de-autor,-documento-e-tabela); [\<table-wrap\>: Tabela](#\<table-wrap\>:-tabela); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id). |
| :---- |

## **\<funding-group\>: Financiamento e Apoio**  {#<funding-group>:-financiamento-e-apoio}

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Zero ou uma vez |

Informação de instituições de apoio, financiamento ou declaração negativa de financiamento, podem aparecer em:

| Elemento | Descrição |
| :---: | ----- |
| [\<ack\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/ack.html) | Agradecimentos |
| \<fn fn-type="financial-disclosure"\> | Nota de declaração de financiamento ou negação de recursos recebidos |
| \<fn fn-type="supported-by"\> | Nota de declaração de apoio |

| Consulte no SPS:  [fn-group\> \+ \<fn\>: Notas de Documento.](#\<fn-group\>-+-\<fn\>:-notas-de-documento)  |
| :---- |

Algumas regras devem ser observadas para a marcação destes dados:

* Quando houver uma ou mais instituições declaradas no texto, obrigatoriamente o uso de [\<funding-source\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/funding-source.html) deve ocorrer.  
* Quando houver uma ou mais instituições declaradas no texto com número de contrato, obrigatoriamente o uso de [\<award-id\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/award-id.html) deve ocorrer.  
* Em todos os casos, o texto originalmente marcado em nota ou agradecimento que contenha a informação de apoio, financiamento ou declaração negativa de financiamento obrigatoriamente deve ser replicado em [\<funding-statement\>](#\<funding-group\>:-financiamento-e-apoio).


| Atenção:  As tags que compõem a marcação de [\<funding-group\>](#\<funding-group\>:-financiamento-e-apoio) não permitem \<label\> ou \<title\>. |
| :---- |


| Consulte na JATS:  [\<ack\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/ack.html); [\<award-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/award-group.html); [\<award-id\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/award-id.html); [\<award-name\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/award-name.html); [\<funding-source\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/funding-source.html); [\<funding-statement\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/funding-statement.html). |
| :---- |

**Exemplos de [\<funding-group\>](#\<funding-group\>:-financiamento-e-apoio) com informação original em nota [\<fn\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento) ou agradecimento [\<ack\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/ack.html).**

**Financiamento:** Informação de uma instituição de financiamento com número de contrato em [nota](#\<fn-group\>-+-\<fn\>:-notas-de-documento) financial-disclosure

```
<front>
    ...
    <article-meta>
        ...     
        </kwd-group>
		<funding-group>
			<award-group>					
				<funding-source> Fundação de Amparo à Pesquisa do Estado de São Paulo FAPESP)</funding-source>
				<award-id>04/08142-0</award-id>
			</award-group>
			<funding-statement><bold>Funding:</bold>This study was supported by Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP - Grant no. 04/08142-0; São Paulo, Brazil)</funding-statement>
		</funding-group>
        ...
    </article-meta>
    ...
</front>
...
<back>
    ...
    <fn-group>
        <fn id="fn1" fn-type="financial-disclosure">
    <label>Funding:</label>
            <p>This study was supported by Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP -Grant no. 04/08142-0; São Paulo, Brazil)</p>
        </fn>
    </fn-group>
    ...
</back>
```

**Financiamento:** Informação de uma instituição de financiamento com número de contrato em agradecimentos [\<ack\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/ack.html)

```
<front>
	...
	<article-meta>
		...					
		</kwd-group>
		<funding-group>						 
			<award-group>                
				<funding-source>Brazilian National Research Council</funding-source>
				<award-id>308059/2022-0</award-id>
			</award-group>
			<funding-statement>The authors would like to thank the support of the Brazilian National Research Council (CNPq; grant 308059/2022-0).</funding-statement>			
		</funding-group>
		...
	</article-meta>
	...
</front>
...
<back>   
	<ack>
		<title>Acknowledgements</title>
		<p>The authors would like to thank the support of the Brazilian National Research Council (CNPq; grant 308059/2022-0).</p>				
	</ack>				
	...
</back>
```

**Financiamento:** Informação de duas instituições de financiamento com o mesmo número de contrato em [nota](#\<fn-group\>-+-\<fn\>:-notas-de-documento) financial-disclosure

```
<front>
    ...
    <article-meta>
        ...     
        </kwd-group>
		<funding-group>
			<award-group>					
				<funding-source>FAPESP</funding-source>
				<funding-source>CAPES</funding-source>
				<award-id>04/08142-0</award-id>
			</award-group>
			<funding-statement><bold>Funding:</bold>This study was supported by FAPESP and CAPES - Grant no. 04/08142-0; São Paulo, Brazil</funding-statement>
		</funding-group>
        ...
    </article-meta>
    ...
</front>
...
<back>
    ...
    <fn-group>
        <fn id="fn1" fn-type="financial-disclosure">
    <label>Funding:</label>
            <p>This study was supported by FAPESP and CAPES - Grant no. 04/08142-0; São Paulo, Brazil</p>
        </fn>
    </fn-group>
    ...
</back>
```

**Financiamento:** Informação de duas instituições de financiamento com os números de contrato distintos em [nota](#\<fn-group\>-+-\<fn\>:-notas-de-documento) financial-disclosure

```
<front>
    ...
    <article-meta>
        ...     
        </kwd-group>
		<funding-group>
			<award-group>					
				<funding-source>FAPESP</funding-source>				
				<award-id>05/07183-0</award-id>
			</award-group>
<award-group>				
				<funding-source>CAPES</funding-source>
				<award-id>04/08142-0</award-id>
			</award-group>
			<funding-statement><bold>Funding:</bold>This study was supported by FAPESP - Grant no. 05/07183-0 and CAPES - Grant no. 04/08142-0; São Paulo, Brazil</funding-statement>
		</funding-group>
        ...
    </article-meta>
    ...
</front>
...
<back>
    ...
    <fn-group>
        <fn id="fn1" fn-type="financial-disclosure">
    <label>Funding:</label>
            <p>This study was supported by FAPESP - Grant no. 05/07183-0 and CAPES - Grant no. 04/08142-0; São Paulo, Brazil</p>
        </fn>
    </fn-group>
    ...
</back>
```

**Apoio:** Informação de uma instituição de apoio em [nota](#\<fn-group\>-+-\<fn\>:-notas-de-documento) supported-by

```
<front>
    ...
    <article-meta>
        ...      
        </kwd-group>
		<funding-group>
<award-group>								
				<funding-source>Fundação de Amparo à Pesquisa do Estado de São Paulo FAPESP)</funding-source>
</award-group>
			<funding-statement>*This study was supported by Fundação de Amparo à Pesquisa do Estado de São Paulo FAPESP) - São Paulo, Brazil</funding-statement>
		</funding-group>
    </article-meta>
    ...
</front>
...
<back>
    ...
    <fn-group>
        <fn id="fn1" fn-type="supported-by">
<label>*</label>
            <p>This study was supported by Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP) - São Paulo, Brazil</p>
        </fn>
    </fn-group>
    ...
</back>
```

**Apoio:** Informação de uma instituição de apoio em agradecimentos [\<ack\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/ack.html)

```
<front>
    ...
    <article-meta>
        ...      
        </kwd-group>
			<funding-group>
<award-group>								
					<funding-source>Fundação de Amparo à Pesquisa do Estado de São Paulo FAPESP)</funding-source>
</award-group>
				<funding-statement>This study was supported by Fundação de Amparo à Pesquisa do Estado de São Paulo FAPESP) - São Paulo, Brazil</funding-statement>
			</funding-group>
    </article-meta>
    ...
</front>
...
<back>
    ...
	<ack>
		<title>Acknowledgements</title>
		<p>This study was supported by Fundação de Amparo à Pesquisa do Estado de São Paulo FAPESP) - São Paulo, Brazil</p>				
	</ack>	
    ...
</back>
```

**Apoio:** Informação de duas instituições de apoio em [nota](#\<fn-group\>-+-\<fn\>:-notas-de-documento) supported-by

```
<front>
    ...
    <article-meta>
        ...     
        </kwd-group>
		<funding-group>
<award-group>	
<funding-source> Coordenação de Aperfeiçoamento de Pessoal de Nível Superior (Capes)</funding-source>
</award-group>
<award-group>	
				<funding-source> Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP)</funding-source>	
</award-group>
			<funding-statement><bold>Support:</bold> This study was supported in part by Coordenação de Aperfeiçoamento de Pessoal de Nível Superior (Capes - Brasília,Brazil), Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP)</funding-statement>
		</funding-group>
        ...
    </article-meta>
    ...
</front>
...
<back>
    ...
    <fn-group>
        <fn id="fn1" fn-type="supported-by">
    <label>Support:</label>
            <p>This study was supported in part by Coordenação de Aperfeiçoamento de Pessoal de Nível Superior (Capes - Brasília,Brazil), Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP)</p>
        </fn>
    </fn-group>
    ...
</back>
```

**Apoio e Financiamento:** Informação de instituição de apoio mais instituição de financiamento com o número de contrato em [nota](#\<fn-group\>-+-\<fn\>:-notas-de-documento) financial-disclosure e [\<ack\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/ack.html).

```
<front>
    ...
    <article-meta>
        ...     
        </kwd-group>
		<funding-group>
<award-group>
<funding-source>Coordenação de Aperfeiçoamento de Pessoal de Nível Superior (Capes)</funding-source>
</award-group>
			<award-group>					
				<funding-source>Fundação de Amparo à Pesquisa do Estado de São Paulo FAPESP)</funding-source>
				<award-id>04/08142-0</award-id>
			</award-group>
			<funding-statement>The authors would like to thank the support of the Brazilian National Research Council.<bold>Funding:</bold>This study was supported in part by Coordenação de Aperfeiçoamento de Pessoal de Nível Superior (Capes - Brasília,Brazil), Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP - Grant no. 04/08142-0; São Paulo, Brazil)</funding-statement>
		</funding-group>
        ...
    </article-meta>
    ...
</front>
...
<back>
<ack>
		<title>Acknowledgements</title>
		<p>The authors would like to thank the support of the Brazilian National Research Council.</p>				
	</ack>
    ...
    <fn-group>
        <fn id="fn1" fn-type="financial-disclosure">
    <label>Funding:</label>
            <p>This study was supported by Fundação de Amparo à Pesquisa do Estado de São Paulo (FAPESP - Grant no. 04/08142-0; São Paulo, Brazil)</p>
        </fn>
    </fn-group>
    ...
</back>
```

**Declaração negativa de financiamento** em [nota](#\<fn-group\>-+-\<fn\>:-notas-de-documento) financial-disclosure

```
<front>
    ...
    <article-meta>
        ...     
        </kwd-group>
	<funding-group>
		<funding-statement><bold>Declaração de financiamento:</bold>Não houve financiamento para esta publicação</funding-statement>
	</funding-group>
        ...
    </article-meta>
    ...
</front>
...
<back>
    ...
    <fn-group>      
    <fn fn-type="financial-disclosure">
        <label>Declaração de financiamento</label>
        <p>Não houve financiamento para esta publicação</p>
    </fn>
    </fn-group>
    ...
</back>
```

## **\<graphic\> e \<inline-graphic\>: Figura** {#<graphic>-e-<inline-graphic>:-figura}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

Atributos obrigatórios:

1. @id  
2. @xlink:href

Os elementos [\<graphic\> e \<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) devem ser usados exclusivamente para figuras e imagens estáticas. O atributo @xlink:href é utilizado para especificar o nome completo (nomeação \+ extensão) da imagem referenciada, a mesma contida no pacote do documento, e só pode conter arquivos com as seguintes extensões:

* **jpg/jpeg** *(preferencialmente use esta extensão)***;**   
* **png;**   
* **tif/tiff;**   
* **svg** \- apenas em [\<alternatives\>](#\<alternatives\>:-.svg).

[\<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) deve ser usado para representar figuras em uma parágrafo. 

* Para acessibilidade, recomenda-se que todas as figuras e imagens venham com sua descrição em [\<alt-text\>](#\<alt-text\>) e/ou [\<long-desc\>](#\<long-desc\>).


| Atenção:  [\<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) não deve ser usado para criar caracteres especiais comuns, como diacríticos e símbolos de direitos autorais; esses caracteres devem ser expressos em [Unicode](#🔹codificação-de-caracteres-especiais);  Figuras em [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) devem aparecer no XML logo abaixo da primeira chamada do texto, independente de onde o dado esteja no PDF, no entanto, apenas quando identificadas fora de [\<app-group\>](#\<app-group\>:-apêndice-e-anexo) e [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar); Para marcação de outros objetos multimídia diferentes de figuras e imagens estáticas, use [\<media\> e \<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia). |
| :---- |

| Consulte no SPS:  [MARCAÇÃO PARA ACESSIBILIDADE](#🔹marcação-para-acessibilidade) [\<alt-text\>](#\<alt-text\>); [\<long-desc\>](#\<long-desc\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). [Encoding e \<\!DOCTYPE\>](#🔹encoding-e-\<!doctype\>); [CODIFICAÇÃO DE CARACTERES ESPECIAIS](#🔹codificação-de-caracteres-especiais); [Nomeação de Arquivos](#nomeação-de-arquivos); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<fig\>: Figura](#\<fig\>:-figura); [\<alternatives\>: .svg](#\<alternatives\>:-.svg). |
| :---- |


**Exemplos:**

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura): em figura [\<fig\>](#\<fig\>:-figura)

```
<fig id="f2">
    <label>Figure 2</label>
    <caption>
        <title>Título da figura</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf2.jpg"/>    
</fig>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura) em figura [\<fig\>](#\<fig\>:-figura) com descrição [\<long-desc\>](#\<long-desc\>)

```
<fig id="f1">
    <label>Figura 1</label>
    <caption>
        <title>Título da Figura</title>
    </caption>
    <graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg">
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
    </graphic>
</fig>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura): em apêndice [\<app\>](#\<app-group\>:-apêndice-e-anexo)

```
<app id="app1">
     <label>Appendix 1</label>	
		<title>Questionnaire for student inclusion</title>	       
      	<graphic xlink:href="1234-5678-scie-58-e1043-gf1.jpg"/>
</app>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura): em material suplementar [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar)

```
<supplementary-material id="suppl2">
<label>Supplementary material</label>
    	<caption>
       	<title>Figure</title>
   	</caption>
   	<graphic xlink:href="1234-5678-scie-58-e1043-gf3.jpg"/>
</supplementary-material>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura): em tabela [\<table-wrap\>](#\<table-wrap\>:-tabela)

```
<table-wrap id="t1">
<label>Table 1</label>
...
<table>
<thead>
...
</thead>
<tbody>
	<tr>
		<td align="left" style="background-color:#EDEDED; "valign="top">
			<bold>1</bold>
		</td>
		<td align="center" style="background-color:#EDEDED; "valign="top">Fenchol</td>
		<td align="center" style="background-color:#EDEDED; "valign="top">C<sub>10</sub>H<sub>18</sub>O</td>
		<td align="center" style="background-color:#EDEDED; "valign="top">154</td>
		<td align="center" style="background-color:#EDEDED; "valign="top">
			<graphic xlink:href="1519-0501-pboci-24-e220168-gf02.jpg"/>
		</td>
	</tr>
...
</table-wrap>
```

[**\<graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura): Para fotos de autores em [\<bio\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/bio.html)

```
<contrib-group>
<contrib contrib-type="author">
		<contrib-id contrib-id-type="orcid">0000-0003-1743-6268</contrib-id>
			<name>
				<surname>Costa</surname>
				<given-names>Mariana</given-names>
			</name>
			<xref ref-type="corresp" rid="c1">*</xref>
			<xref ref-type="aff" rid="aff1">a</xref>
			<bio>
			<p><graphic xlink:href="0103-5053-jbchs-35-09-e-20240042-gf13.jpg"/></p> <p>Phasellus ac iaculis nisl. Integer dictum odio et tristique semper suspendisse
potenti, maecenas consectetur fermentum nisi eu commodo. Phasellus at mollis exvivamus sit amet imperdiet est.</p>
			</bio>
</contrib>
</contrib-group>
```

[**\<inline-graphic\>**](#\<graphic\>-e-\<inline-graphic\>:-figura): Compondo trecho de um parágrafo \<p\>

```
<p>Phasellus ac iaculis nisl. Integer dictum odio et tristique semper suspendisse potenti, maecenas <inline-graphic xlink:href="1234-5678-scie-58-e1043-gf17.jpg"/> consectetur fermentum nisi eu commodo. Phasellus at mollis exvivamus sit amet imperdiet est.</p>
```

## **\<history\>: Datas de Histórico**  {#<history>:-datas-de-histórico}

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Zero ou uma vez |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Zero ou uma vez |

Agrupa as datas de histórico do documento, tais como: data de recebido, aceito, revisado, quando o documento foi publicado em [preprint](#preprint:-documentos-publicados-anteriormente-como-preprint), se teve correção ([errata](#errata) ou [adendo](#adendo)), [retratação](#retratação), [manifestação de preocupação](#manifestação-de-preocupação), etc. 

Atributos e valores obrigatórios (exceto para [errata](#errata), [retratação](#retratação), [adendo](#adendo), [manifestação de preocupação](#manifestação-de-preocupação) e [parecer](#parecer:-revisão-por-pares-aberta)):

1. Em [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html) @date-type="received"  
2. Em [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html) @date-type="accepted"

Os valores possíveis para o atributo @date-type são:

| Valor | Descrição |
| :---: | ----- |
| **accepted** | Data em que um manuscrito foi aceito. |
| **corrected** | Data de aprovação de uma Errata ou Adendo para um documento publicado. |
| **expression-of-concern** | Data de aprovação de uma Manifestação de Preocupação para um documento publicado. |
| **pub** | Data de publicação (eletrônica ou impressa). |
| **preprint** | Data de publicação do documento enquanto preprint. |
| **received** | Data em que um manuscrito foi recebido. |
| **resubmitted** | A data em que um documento, normalmente um manuscrito, foi reenviado para publicação. |
| **retracted** | Data de aprovação de uma retratação (total ou parcial) para um documento publicado.  |
| **rev-recd** | Data em que um manuscrito revisado foi recebido. |
| **rev-request** | Data de solicitação das revisões do manuscrito. |
| **reviewer-report-received** | Data em que um [parecer](#parecer:-revisão-por-pares-aberta) foi enviado para um manuscrito. Exclusivamente usada para documentos de parecer com @article-type="reviewer-report". |

As tags [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html), [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) e [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html) obrigatoriamente devem estar presente nas datas @date-type com valores:

1. @date-type="received"  
2. @date-type="accepted"  
3. @date-type="corrected"  
4. @date-type="retracted"  
5. @date-type="expression-of-concern"

Para todos os outros tipos de datas é mandatória a presença de ao menos [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html).

| Atenção:  As tags de [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html) e [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) obrigatoriamente devem possuir dois dígitos: 01, 02 ... 12, etc; Se houver datas de histórico distintas entre documento no idioma original e tradução, a tag de [\<history\>]() deve ocorrer em [\<article\>](#\<article\>:-artigo) e [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<history>
        <date date-type="received">
            <day>15</day>
            <month>03</month>
            <year>2024</year>
        </date>
        <date date-type="accepted">
            <day>12</day>
            <month>05</month>
            <year>2024</year>
        </date>
        <date date-type="preprint">
            <day>21</day>
            <month>09</month>
            <year>2023</year>
        </date>
        <date date-type="corrected">
            <day>03</day>
            <month>07</month>
            <year>2025</year>
        </date>
</history>
```

### 

| Consulte:  [Comunicado](https://us4.campaign-archive.com/?u=f26dcf71797dd37381acb4aa5&id=2a6634a845) sobre a obrigatoriedade de datas completas, enviado em 21/10/2016; [Guia para publicação de Adendo](https://wp.scielo.org/wp-content/uploads/guia_adendo.pdf); [Guia para publicação de Errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf); [Guia para Publicação de Retratação](https://wp.scielo.org/wp-content/uploads/guia_retratacao.pdf); [Guia para publicação de Manifestação de Preocupação](https://wp.scielo.org/wp-content/uploads/guia_manifestacao.pdf). |
| :---- |

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html); [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html); [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html); [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html). |
| :---- |

| Consulte no SPS:  [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [XML do documento mencionado pelo Adendo](#xml-do-documento-mencionado-pelo-adendo); [XML do documento mencionado pela Errata](#xml-do-documento-mencionado-pela-errata); [XML do documento Retratado Totalmente](#xml-do-documento-retratado-totalmente); [XML do documento Retratado Parcialmente](#xml-do-documento-retratado-parcialmente); [Preprint: documentos publicados anteriormente como Preprint](#preprint:-documentos-publicados-anteriormente-como-preprint); [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta). |
| :---- |

## **\<issue\>: Número, Número Especial e Suplemento** {#<issue>:-número,-número-especial-e-suplemento}

***e exemplos de volume \<volume\>*** 

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Zero ou uma vez |
| [\<element-citation\>](#\<ref-list\>:-lista-de-referências) | Zero ou uma vez |

Identifica o número de uma publicação periódica. Também serve para identificação de suplemento ou número especial, quando existente em \<article-meta\>. 

Em \<article-meta\> considerar para [\<issue\>](#\<issue\>:-número,-número-especial-e-suplemento):

| Número | Exemplo de marcação em \<issue\> |
| :---: | :---: |
| Número | \<issue\>**4**\</issue\> |
| Suplemento de número | \<issue\>**4 suppl 1**\</issue\> |
| Suplemento de volume | \<issue\>**suppl 1**\</issue\> |
| Número especial | \<issue\>**spe1**\</issue\> |

| Atenção:  A pontuação, espaços e o formato do texto são importantes, não use pontuação, caixa alta ou zero a esquerda e respeite os espaços designados para cada conteúdo em [\<issue\>](#\<issue\>:-número,-número-especial-e-suplemento); Sempre informe o número do suplemento ou número especial, incluindo também no PDF, que será o dado fonte para a marcação do XML. Exemplos para legenda bibliográfica: Para suplemento: supl. 1, suppl. 1,  s1, etc; Para número especial: nesp. 1, nspe.1,  spe. 1, esp.1, etc. É proibido o uso do elemento \<supplement\> em \<article-meta\>. Suplementos devem ser identificados em [\<issue\>](#\<issue\>:-número,-número-especial-e-suplemento). |
| :---- |

**Exemplos de \<volume\> e \<issue\> em \<article-meta\>:**

**Volume e número:** v4n10

```
<front>
    ...
    <article-meta>
        ...   
	  <volume>10</volume>
        <issue>4</issue>
   ...
    </article-meta>
    ...
</front>
```

**Número sem volume:** n4

```
<front>
    ...
    <article-meta>
        ...   
        <issue>4</issue>
        ...
    </article-meta>
    ...
</front>
```

**Volume sem número:** v10

```
<front>
    ...
    <article-meta>
        ...
        <volume>10</volume>       
        ...
    </article-meta>
    ...
</front>
```

**Suplemento de volume:** v10s2

```
<front>
    ...
    <article-meta>
        ...
        <volume>10</volume>
        <issue>suppl 2</issue>
        ...
    </article-meta>
    ...
</front>
```

**Suplemento de número:** v10n4s2

```
<front>
    ...
    <article-meta>
        ...
        <volume>10</volume>
        <issue>4 suppl 2</issue>
        ...
    </article-meta>
    ...
</front>
```

**Volume com número especial:** v10nspe1

```
<front>
    ...
    <article-meta>
        ...
        <volume>10</volume>
        <issue>spe1</issue>
        ...
    </article-meta>
    ...
</front>
```

| Consulte no SPS:  [REGRAS PARA NOMEAÇÃO DE ARQUIVOS E PASTAS](#regras-para-nomeação-de-arquivos-e-pastas); [\<ref-list\>: Lista de Referências](#\<ref-list\>:-lista-de-referências). |
| :---- |

## **\<journal-meta\>: Metadados do Periódico** {#<journal-meta>:-metadados-do-periódico}

| Aparece em | Ocorre |
| :---: | :---: |
| \<front\> | Uma vez |

São identificados os metadados do periódico registrados na SciELO. É composto por:

| Elemento | Atributo e valor | Descrição |
| :---: | ----- | ----- |
|  **\<journal-id\>** | @journal-id-type="nlm-ta" | Apenas se o periódico for indexado no PubMed**1**.   |
|  | @journal-id-type="publisher-id" | Acrônimo oficial do periódico na SciELO. |
| **\<journal-title\>** | *não se aplica* | Título do periódico registrado na SciELO. |
| **\<abbrev-journal-title\>** | @abbrev-type="publisher" | Título abreviado do periódico registrado na SciELO. |
|  **\<issn\>** | @pub-type="epub" | ISSN online do periódico. |
|  | @pub-type="ppub" | ISSN print do periódico. |
| **\<publisher-name\>** | *não se aplica* | Nome do publisher do periódico registrado na SciELO. |

 **1**Usar título abreviado do periódico registrado no [PubMed](https://pubmed.ncbi.nlm.nih.gov/).

**Exemplo:**

```
<journal-meta>
     <journal-id journal-id-type="nlm-ta">Braz J Med Biol Res</journal-id>
     <journal-id journal-id-type="publisher-id">bjmbr</journal-id>
     <journal-title-group>
          <journal-title>Brazilian Journal of Medical and Biological Research</journal-title>
          <abbrev-journal-title abbrev-type="publisher">Braz. J. Med. Biol. Res.</abbrev-journal-title>
     </journal-title-group>
     <issn pub-type="epub">1414-431X</issn>
     <issn pub-type="ppub">0100-879X</issn>
     <publisher>
          <publisher-name>Associação Brasileira de Divulgação Científica</publisher-name>
     </publisher>
</journal-meta>
```

Encontra-se disponível em formato csv uma lista de metadados de periódicos necessários para identificação de elementos em [\<journal-meta\>](#\<journal-meta\>:-metadados-do-periódico). O documento pode ser baixado a partir do [link title-tab-v2](http://static.scielo.org/sps/titles-tab-v2-utf-8.csv) e sua atualização é semanal, sempre às quartas-feiras.

| Atenção:  Para fazer o download do csv [link title-tab-v2](http://static.scielo.org/sps/titles-tab-v2-utf-8.csv), copie o link e coloque no navegador, o arquivo irá baixar automaticamente. |
| :---- |

| Consulte:  [PubMed](https://pubmed.ncbi.nlm.nih.gov/); [title-tab-v2](http://static.scielo.org/sps/titles-tab-v2-utf-8.csv). |
| :---- |

## **\<list\>: Lista** {#<list>:-lista}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<app\>](#\<app-group\>:-apêndice-e-anexo) | Zero ou mais vezes |
| \<body\> | Zero ou mais vezes |
| [\<boxed-text\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/boxed-text.html) | Zero ou mais vezes |
| [\<disp-quote\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/disp-quote.html) | Zero ou mais vezes |
| [\<list-item\>](#\<list\>:-lista) | Zero ou mais vezes |
| \<p\> | Zero ou mais vezes |
| [\<sec\>](#\<sec\>:-seção-de-texto) | Zero ou mais vezes |

Elemento utilizado para identificação de uma lista que contém dois ou mais itens. 

Atributo obrigatório

1. @list-type

* O uso de \<label\> é menos acessível e legível por máquina para representar os rótulos da lista sendo o atributo @list-type o mais adequado para a representação automática dos rótulos, por este motivo \<label\> não deve ser usado. O uso de \<title\> para identificar o título da lista deve ser usado quando disponível a informação.

Os valores possíveis para o atributo @list-type são:

| Valor  | Descrição |
| :---: | ----- |
| **order** | Lista ordenada, cujo prefixo é um número. |
| **bullet** | Lista desordenada, cujo prefixo utilizado é um ponto, barra ou outro símbolo. |
| **alpha-lower** | Lista ordenada, cujo prefixo é um caractere alfabético minúsculo. |
| **alpha-upper** | Lista ordenada, cujo prefixo é um caractere alfabético maiúsculo. |
| **roman-lower** | Lista ordenada, cujo prefixo é um numeral romano minúsculo. |
| **roman-upper** | Lista ordenada, cujo prefixo é um numeral romano maiúsculo. |
| **simple** | Lista simples, sem prefixo nos itens. |

**Exemplos:**

**lista com bullet: @list-type="bullet"**

```
<list list-type="bullet">
  <title>Nam commodo</title>
    <list-item>
      <p>Morbi luctus elit enim.</p>
    </list-item>
    <list-item>
      <p>Nullam nunc leo.</p>
    </list-item>
    <list-item>
      <p>Proin id dui lorem.</p>
    </list-item>
    <list-item>
      <p>Nunc finibus risus.</p>
    </list-item>
</list>
```

**lista numérica @list-type="order" com sub-item (\<list-item\> dentro de \<list-item\>)**

```
<list list-type="order">
  <title>Vivamus cursus</title>
    <list-item>
      <p>Nullam gravida tellus eget condimentum egestas.</p>
        <list list-type="order">
          <list-item>
            <p>Curabitur luctus lorem ac feugiat pretium.</p>
          </list-item>
        </list>
    </list-item>
    <list-item>
      <p>Donec pulvinar odio ut enim lobortis, eu dignissim elit accumsan.</p>
    </list-item>
</list>
```

| Atenção:  Os valores de @list-type criam automaticamente seu valor, exemplo: se @list-type="order" a apresentação será em cada [\<list-item\>](#\<list\>:-lista) o valor numérico 1, 2 e assim por diante. Por isso, não identifique os rótulos para estes dados dentro de [\<list-item\>](#\<list\>:-lista) e não use \<label\>. |
| :---- |

## **\<media\> e \<inline-media\>: Objeto Multimídia**   {#<media>-e-<inline-media>:-objeto-multimídia}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

Referencia um arquivo externo que contém um objeto multimídia, tais como: animação, filme, áudio, documento, planilha, etc., exceto figuras e imagens estáticas. Para isso, use [\<graphic\> e \<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura).

Atributos obrigatórios:

1. @mime-type   
2. @mime-subtype  
3. @xlink:href

Em @mime-subtype deve ser inserido a extensão dos arquivos, exemplo: pdf, mp4, xlsx, etc. Os formatos mais comuns para @mime-type são:

| Formato | Valor @mime-type |
| ----- | :---: |
| **Planilha Excel** | application |
| **Documento Word** | application |
| **Apresentação Power Point** | application |
| **PDF** | application |
| **Vídeo** | video |
| **Áudio** | audio |
| **arquivo(s) compactado(s)** | application |

Para outros formatos de @mime-type e @mime-subtype consultar os valores possível em [Media Types;](https://www.iana.org/assignments/media-types/media-types.xhtml)

Para **@mime-subtype** é obrigatório as seguintes extensões para os formatos:

* **Vídeo:** mp4;  
* **Aúdio:** mp3;  
* **arquivo(s) compactado(s):** zip. 

O atributo @xlink:href é utilizado para especificar o nome completo (nomeação \+ extensão) da imagem referenciada, a mesma contida no pacote do documento.

* Para acessibilidade recomenda-se que vídeos e áudios venham com sua descrição em [\<alt-text\>](#\<alt-text\>) e/ou [\<long-desc\>](#\<long-desc\>) mais a transcrição do conteúdo na seção [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>).

| Consulte:  [Media Types](https://www.iana.org/assignments/media-types/media-types.xhtml); [PMC: Supplementary Material](https://www.ncbi.nlm.nih.gov/pmc/pmcdoc/tagging-guidelines/article/dobs.html#dob-suppm). |
| :---- |

| Consulte no SPS:  [MARCAÇÃO PARA ACESSIBILIDADE](#🔹marcação-para-acessibilidade) [\<alt-text\>](#\<alt-text\>); [\<long-desc\>](#\<long-desc\>); [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>); [\<long-desc\>, \<alt-text\> e \<sec sec-type="transcript"\>: Principais características e marcação combinada](#\<long-desc\>,-\<alt-text\>-e-\<sec-sec-type="transcript"\>:-principais-características-e-marcação-combinada). [Nomeação de Arquivos](#nomeação-de-arquivos); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar). |
| :---- |

**Exemplos:**

[**\<media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia)**:** em [material suplementar](#\<supplementary-material\>:-material-suplementar)

```
<supplementary-material id="suppl1">
<label>Supplementary material 1</label>
            <caption>
                <title>Video 1</title>
            </caption>
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4"/>
</supplementary-material>
<supplementary-material id="suppl3">
<label>Supplementary material 3</label>
             <caption>
                <title>Spreadsheet 1</title>
             </caption>
<media mimetype="application" mime-subtype="xlsx" xlink:href="1234-5678-scie-58-e1043-md2.xlsx"/>
</supplementary-material>
```

[**\<media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia)**:** no corpo do texto com descrição [\<long-desc\>](#\<long-desc\>) e referência cruzada @ref-type="sec"para a seção de transcrição [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>)

```
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<label>Video 1</label>
 		<caption>
 			 <title>Vídeo: malesuada vehicula</title> 
 </caption> 
<attrib>Fonte: consectetur adipiscing elit</attrib>
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
<xref ref-type="sec" rid="TR1"/>
</media>
```

[**\<inline-media\>**](#\<media\>-e-\<inline-media\>:-objeto-multimídia)**:** em parágrafo

```
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque dui eros, laoreet eget sem nec, cursus vulputate tellus <inline-media mimetype="application" mime-subtype="pdf" xlink:href="1234-5678-scie-58-e1043-md1.pdf">Document<inline-media>, elit erat malesuada magna, in tempor urna nunc eget leo.</p>
```

## **\<permissions\>: Licença Creative Commons e Copyright** {#<permissions>:-licença-creative-commons-e-copyright}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Uma vez |

A permissão é um conjunto de condições sob as quais o conteúdo do documento pode ser usado, acessado e distribuído. Para SciELO Brasil é obrigatório a declaração do elemento com a atribuição [CC-BY.](https://creativecommons.org/licenses/by/4.0/deed.pt)

Atributos obrigatórios em [\<license\>](#\<permissions\>:-licença-creative-commons-e-copyright):

1. @license-type="open-access"  
2. @xlink:href  
3. @xml:lang

O valor para @xml:lang deve ser o correspondente ao idioma do texto da licença, o mesmo idioma do documento ou em inglês. Para @xlink:href use o link da licença correspondente ao idioma descrito em @xml:lang. 

| Idioma | Link da licença CC-BY correspondente para @xlink:href |
| :---: | :---- |
| Português | [https://creativecommons.org/licenses/by/4.0/deed.pt](https://creativecommons.org/licenses/by/4.0/deed.pt) |
| Inglês | [https://creativecommons.org/licenses/by/4.0/deed.en](https://creativecommons.org/licenses/by/4.0/deed.en) |
| Espanhol | [https://creativecommons.org/licenses/by/4.0/deed.es](https://creativecommons.org/licenses/by/4.0/deed.es) |

O texto descrito em [\<license-p\>](#\<permissions\>:-licença-creative-commons-e-copyright) deve ser o mesmo texto descrito no PDF do documento (quando houver). Caso o PDF não indique um texto e use, por exemplo, apenas o logo do CC-BY, use o texto padrão em inglês: This is an open-access article distributed under the terms of the Creative Commons Attribution License.

Quando o PDF do documento apresentar declaração de Copyright, estes dados obrigatoriamente devem ser marcados em [\<copyright-statement\>](http://jats.nlm.nih.gov/publishing/tag-library/1.3/element/copyright-statement.html), se houver informação de ano adicionada-se a tag  [\<copyright-year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/copyright-year.html)  e se houver informação do detentor do copyright, adiciona-se a tag  [\<copyright-holder\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/copyright-holder.html).

**Exemplos:**

**Licença sem indicação de Copyright**

```
<permissions>
	<license license-type="open-access" xlink:href="https://creativecommons.org/licenses/by/4.0/" xml:lang="en">
		<license-p>This is an open-access article distributed under the terms of the Creative Commons Attribution License</license-p>
	</license>
</permissions>
```

**Licença com Copyright: ano e detentor**

```
<permissions>
<copyright-statement>Copyright © 2025, the authors</copyright-statement>
<copyright-year>2025</copyright-year>
<copyright-holder>the authors</copyright-holder>
	<license license-type="open-access" xlink:href="https://creativecommons.org/licenses/by/4.0/" xml:lang="en">
		<license-p>This is an open-access article distributed under the terms of the Creative Commons Attribution License</license-p>
	</license>
</permissions>
```

**Licença com Copyright: apenas ano**

```
<permissions>
<copyright-statement>Copyright © 2025</copyright-statement>
<copyright-year>2025</copyright-year>
	<license license-type="open-access" xlink:href="https://creativecommons.org/licenses/by/4.0/" xml:lang="en">
		<license-p>This is an open-access article distributed under the terms of the Creative Commons Attribution License</license-p>
	</license>
</permissions>
```

| Atenção:  Recomenda-se o uso dos logos oficiais do Creative Commons, para baixar os logos acesse: [download CC](https://creativecommons.org/mission/downloads/). |
| :---- |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*:* *2.3. Critérios SciELO Brasil e o modus operandi de Ciência Aberta, 5.2.4. Relevância, sustentabilidade e qualificação editorial* e *5.2.10.1 Interoperabilidade – resumo das condições metodológicas.* |
| :---- |

| Consulte na JATS:  [\<copyright-statement\>](http://jats.nlm.nih.gov/publishing/tag-library/1.3/element/copyright-statement.html); [\<copyright-holder\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/copyright-holder.html); [\<copyright-year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/copyright-year.html). |
| :---- |

## **\<product\>: Resenha de Livro** {#<product>:-resenha-de-livro}

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Zero ou mais vezes |

Atributo obrigatório:

1. @product-type="book"

O elemento serve para marcação da referência da resenha, quando esta for relacionada a um livro ou capítulo de livro, onde o tipo de documento em [\<article\>](#\<article\>:-artigo) terá o atributo com valor igual a @article-type="book-review". Para este caso, o atributo e valor em [\<product\>](#\<product\>:-resenha-de-livro) será: @product-type="book".

**Exemplo @product-type="book"**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="book-review" xml:lang="en">
...
<product product-type="book">
    <person-group person-group-type="author">
        <name>
            <surname>ONFRAY</surname>
            <given-names>Michel</given-names>
        </name>
    </person-group>
    <source>La comunidad filosófica: manifiesto por una universidad popular</source>
    <person-group person-group-type="translator">
        <name>
            <surname>Castro</surname>
            <given-names>Antonia García</given-names>
        </name>
    </person-group>
    <publisher-loc>Barcelona</publisher-loc>
    <publisher-name>Gedisa</publisher-name>
    <year>2008</year>
    <size units="pages">155</size>
    <isbn>78-84-9784-252-5</isbn>
</product>
...
</article>
```

| Consulte no SPS:  [\<article\>: Artigo](#\<article\>:-artigo). |
| :---- |

## **\<pub-date\>: Datas de Publicação**  {#<pub-date>:-datas-de-publicação}

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Uma ou mais vezes |

Atributos obrigatórios:

1. @date-type="pub"  
2. @date-type="collection"  
3. @publication-format="electronic"

Representam as datas de publicação do documento em SciELO @date-type="pub" e do número ao qual pertencem com base em sua periodicidade @date-type="collection".

* Em [\<pub-date publication-format="electronic" date-type="pub"\>](#\<pub-date\>:-datas-de-publicação) é obrigatório constar as tags [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html) \+ [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) \+  [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html);  
* Em [\<pub-date publication-format="electronic" date-type="collection"\>](#\<pub-date\>:-datas-de-publicação) é obrigatório constar as tags [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html) com adição quando necessário de [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) e/ou [\<season\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/season.html), não é permitido [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html).

**Exemplos:**

**Periódico Bimestral:** intervalo de meses e ano \- [\<season\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/season.html) \+ [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html)

```
<pub-date publication-format="electronic" date-type="pub">
   <day>01</day>
   <month>01</month>
   <year>2025</year>
</pub-date>
<pub-date publication-format="electronic" date-type="collection">
   <season>Jan-Feb</season>
   <year>2025</year>
</pub-date>
```

**Periódico Mensal:** mês e ano \- [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) \+ [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html)

```
<pub-date publication-format="electronic" date-type="pub">
   <day>01</day>
   <month>01</month>
   <year>2025</year>
</pub-date>
 <pub-date publication-format="electronic" date-type="collection">
   <month>01</month>
   <year>2025</year>
</pub-date>
```

**Periódico Anual:** ano \- [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html)

```
<pub-date publication-format="electronic" date-type="pub">
   <day>01</day>
   <month>01</month>
   <year>2025</year>
</pub-date>
<pub-date publication-format="electronic" date-type="collection">  
   <year>2025</year>
</pub-date>
```

| Atenção:  Se o periódico, em volume anual, indicar mês ou intervalo de meses no PDF, a tag de [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) ou [\<season\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/season.html) deverá ser adicionada na data do tipo collection; Para as tags de [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html) e [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) sempre usar dois dígitos numéricos: 01, 02...11, etc.; Para datas do tipo pub, criar as tags [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html) e [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) com informação 00 ou qualquer outra data para que seja alterada posteriormente com a data efetiva da publicação do documento pela unidade de produção SciELO; Para datas do tipo collection, sempre preencher a data relacionada ao volume/números ao qual pertence o documento, seguindo sua periodicidade. |
| :---- |

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html); [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html); [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html); [\<season\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/season.html); [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html). |
| :---- |

## **\<ref-list\>: Lista de Referências**  {#<ref-list>:-lista-de-referências}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| \<back\> | Uma ou mais vezes |
| [\<ref-list\>](#\<ref-list\>:-lista-de-referências) | Uma ou mais vezes |

Atributo obrigatório em [\<element-citation\>](#\<ref-list\>:-lista-de-referências):

1. @publication-type

[\<ref-list\>](#\<ref-list\>:-lista-de-referências) é um dado obrigatório em todos os documentos indexáveis para a coleção SciELO Brasil, exceto em errata, retratação, adendo, manifestação de preocupação e parecer. Representa o conjunto de referências em \<back\> de um documento em qualquer norma e deve conter, obrigatoriamente, o elemento [\<ref\>](#\<ref-list\>:-lista-de-referências), que por sua vez, obrigatoriamente contém os elementos [\<mixed-citation\>](#\<ref-list\>:-lista-de-referências) e [\<element-citation\>](#\<ref-list\>:-lista-de-referências). 

* Em [\<mixed-citation\>](#\<ref-list\>:-lista-de-referências) preserva-se a referência com sua formatação, incluindo itálico, negrito, sobrescrito, subscrito, espaços e pontuação. É utilizada para apresentação do dado na plataforma SciELO.  
* Em [\<element-citation\>](#\<ref-list\>:-lista-de-referências) identifica-se a referência de forma detalhada com a marcação dos elementos que a compõem. É utilizada para geração de métricas.

Os valores possíveis para o atributo @publication-type em [\<element-citation\>](#\<ref-list\>:-lista-de-referências) são:

| Valor | Descrição |
| :---: | ----- |
| **book** | Referencia livros. Pode também representar somente uma parte ou capítulo de um livro. (Veja também [\<product\>: Resenha](#\<product\>:-resenha-de-livro)) |
| **confproc** | Identifica documentos relacionados à eventos científicos: atas, anais, resultados, proceedings, convenções, conferências entre outros. |
| **data** | Referencia dados de pesquisa ([dataset](#declaração-de-disponibilidade-de-dados)). |
| **database** | Referencia bases de dados. |
| **journal** | Referencia artigos em periódicos científicos. |
| **legal-doc** | Referencia normas jurídicas. |
| **letter** | Referencia cartas e outras comunicações pessoais. |
| **newspaper** | Referencia artigos de jornal. |
| **patent** | Referencia patentes. |
| **preprint** | Referencia documentos publicados como [preprints](#preprint:-documentos-publicados-anteriormente-como-preprint). |
| **report** | Referencia um relatório técnico, normalmente de autoria institucional. |
| **software** | Referencia um software em suportes como CDs, DVDs, suporte online, dispositivos USB, etc. |
| **thesis** | Referencia monografias, dissertações ou teses para obtenção de um grau acadêmico (livre-docência, doutorado, mestrado, bacharelado, licenciatura, etc.). |
| **webpage** | Referencia conteúdos de web sites e blogs. |
| **other** | Referencia tipos não previstos pelo [SPS](#guia-para-o-uso-de-elementos-e-atributos-xml-em-documentos-que-seguem-a-implementação-scielo-publishing-schema). |

| Atenção:  Em [\<element-citation\>](#\<ref-list\>:-lista-de-referências): Não incluir todo um texto em um elemento com formatação \<italic\> e \<bold\>; Nunca utilizar pontuação (ponto final, vírgula, etc.) entre os elementos dentro de [\<element-citation\>](#\<ref-list\>:-lista-de-referências); Caso não exista um elemento específico para determinada informação, marque o dado em \<comment\>; É proibido o uso de \<comment\> abarcando a tag [\<ext-link\>](#\<ext-link\>:-link): Não usar  \<comment\>\<ext-link\>https://www…\</ext-link\>\</comment\>; Pode usar: \<comment\>texto\<ext-link\>https://www…\</ext-link\>\</comment\> (Usualmente utilizado para inserir o texto Disponível em:, Acessado em:, etc.); É proibido haver a ocorrência de dois links [\<ext-link\>](#\<ext-link\>:-link) em [\<element-citation\>](#\<ref-list\>:-lista-de-referências). Em [\<mixed-citation\>](#\<ref-list\>:-lista-de-referências): Não taguear elementos dentro de [\<mixed-citation\>](#\<ref-list\>:-lista-de-referências) exceto para formatação \<bold\>, \<italic\>, \<sup\> e \<sub\>. Esta tag é usada para a apresentação da referência na interface SciELO e segue a folha de estilo da interface; É proibido haver a ocorrência de dois links [\<ext-link\>](#\<ext-link\>:-link) em [\<mixed-citation\>](#\<ref-list\>:-lista-de-referências). |
| :---- |

**Regras Gerais aplicáveis em elementos que compõem as referências em [\<element-citation\>](#\<ref-list\>:-lista-de-referências)**:

Espera-se que todas as referências (quando utilizado normas bibliográficas sem adaptações) tenham a indicação do nome da fonte que está sendo citada [\<source\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/source.html) e pelo menos a indicação de ano [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html) desta fonte. A tabela a seguir mostra alguns dos elementos mais comuns que podem ocorrer nas referências, em especial as do tipo journal e book.

| \<element-citation\> |  |
| ----- | ----- |
| **Elemento** | **Descrição** |
| [\<person-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/person-group.html) \+ [\<name\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/name.html) | Autoria pessoa física  |
| [\<person-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/person-group.html) \+ [\<collab\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/collab.html) | Autoria institucional |
| [\<source\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/source.html) | Título do periódico |
| [\<volume\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/volume.html) | Volume |
| [\<issue\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/issue.html) | Número |
| [\<supplement\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/supplement.html) | Suplemento |
| [\<fpage\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/fpage.html) | Primeira página |
| [\<lpage\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/lpage.html) | Última página |
| [\<size units="pages"\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/size.html) | Quantidade total de páginas do objeto citado |
| [\<elocation-id\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/elocation-id.html) | Paginação digital (elocation-id) |
| [\<edition\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/edition.html) | Edição |
| [\<version\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/version.html) | Versão |
| [\<pub-id\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/pub-id.html) | Identificador de uma publicação em uma referência bibliográfica, como por exemplo número DOI. |
| [\<day\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/day.html) | Dia do objeto citado |
| [\<month\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/month.html) | Mês do objeto citado |
| [\<year\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/year.html) | Ano do objeto citado |
| [\<date-in-citation content-type="access-date"\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date-in-citation.html) | Data em que foi acessada a citação pelo(s) autor(es) |
| [\<ext-link\>](#\<ext-link\>:-link) | link url  |
| [\<publisher-name\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/publisher-name.html) | Nome da editora/publicadora |
| [\<publisher-loc\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/publisher-loc.html) | Cidade, estado e/ou país editora/publicadora |

Quando houver a indicação de autoria, [\<person-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/person-group.html) com o atributo @person-group-type é requerido sem nenhum outro atributo adicional, sendo obrigatório o valor author quando houver autoria institucional [\<collab\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/collab.html). Os valores possíveis para @person-group-type de autores pessoas físicas em [\<element-citation\>](#\<ref-list\>:-lista-de-referências) são:

| Valor | Descrição |
| :---: | ----- |
| **author** | autor |
| **editor** | editor |
| **translator** | tradutor |
| **compiler** | compilador do conteúdo |

Quando [\<person-group\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/person-group.html) indicar [\<name\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/name.html), [\<surname\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/surname.html) é requerido. Neste caso, quando um autor pessoa física for reconhecido com apenas um nome, este nome deve ser marcado em **[\<surname\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/surname.html)** e não em [\<given-names\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/given-names.html), exemplo:

```
<person-group person-group-type="author">
<name>
<surname>Cher</surname>
</name>					
</person-group>
```

Para marcar títulos dos objetos que estão sendo referenciados use:

* [\<article-title\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/article-title.html) para journal: Título do artigo;  
* [\<part-title\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/part-title.html) para book: Título do capítulo do livro *(não use \<chapter-title\>);*  
* [\<data-title\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/data-title.html) para data: Título do conjunto de dados (dataset);  
* [\<conf-name\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/conf-name.html) para confproc: Título do evento (usualmente acompanhado também dos elementos [\<conf-loc\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/conf-loc.html),  [\<conf-date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/conf-date.html), [\<conf-num\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/conf-num.html) e [\<conf-sponsor\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/conf-sponsor.html))

A data de citação [\<date-in-citation\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date-in-citation.html) obrigatoriamente deve possuir o atributo e valor @content-type="access-date", sem nenhum outro atributo adicional.

A quantidade total de páginas [\<size\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/size.html) obrigatoriamente deve possuir o atributo e valor @units="pages", sem nenhum outro atributo adicional.

Sempre que houver a indicação de primeira página [\<fpage\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/fpage.html), o elemento de última página [\<lpage\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/lpage.html) é requerido.

[\<pub-id\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/pub-id.html) com o atributo e valor @pub-id-type="doi" é mais comumente usado para marcação de DOI nas referências, no entanto, @pub-id-type pode ocorrer com outros valores [permitidos pela JATS](https://jats.nlm.nih.gov/publishing/tag-library/1.3/attribute/pub-id-type.html). O mesmo ocorre com [\<ext-link\>](#\<ext-link\>:-link) o atributo @ext-link-type="uri" e @ext-link-type="doi" são os mais comumente utilizados, mas pode ocorrer com outros valores [permitidos pela JATS](https://jats.nlm.nih.gov/publishing/tag-library/1.3/attribute/ext-link-type.html).

**Exemplos:**

**Artigo de periódico: @publication-type="journal"**

```
<ref-list>
	<title>Referências</title>
	<ref-list>
			<ref id="B1">
			<mixed-citation>Benchimol M, Souza W de. Endocytosis in anaerobic parasitic protists. Mem Inst Oswaldo Cruz. 2024;119:e240058. DOI: 10.1590/0074-02760240058, PMCID: PMC11285859,PMID: 39082582. Available from: https://www.scielo.br/j/mioc/a/fcqXp9PvZRsgBPV9hGV5PpL/?lang=en</mixed-citation>
			<element-citation publication-type="journal">
				<person-group person-group-type="author">
					<name>
						<surname>Benchimol</surname>
						<given-names>M</given-names>
					</name>
					<name>
						<surname>Souza</surname>
						<given-names>W de</given-names>
					</name>
				</person-group>
				<article-title>Endocytosis in anaerobic parasitic protists</article-title>
				<source>J Aging Res</source>
				<year>2024</year>
				<volume>119</volume>
				<elocation-id>e240058</elocation-id>
<pub-id pub-id-type="doi">10.1590/0074-02760240058</pub-id>
<pub-id pub-id-type="pmcid">PMC11285859</pub-id>
<pub-id pub-id-type="pmid">39082582</pub-id>
				<ext-link ext-link-type="uri" xlink:href="https://www.scielo.br/j/mioc/a/fcqXp9PvZRsgBPV9hGV5PpL/?lang=en">https://www.scielo.br/j/mioc/a/fcqXp9PvZRsgBPV9hGV5PpL/?lang=en</ext-link>
			</element-citation>
		</ref>  				
</ref-list>
```

**Livro:** **@publication-type="book"**

```
<ref id="B2">
        <label>2</label>
        <mixed-citation>Hamric, Ann B.; Spross, Judith A.; Hanson, Charlene M. Advanced practice nursing: an integrative approach. 3rd ed. St. Louis (MO): Elsevier Saunders; c2005. 979 p.</mixed-citation>
        <element-citation publication-type="book">
          <person-group person-group-type="author">
            <name>
                <surname>Hamric</surname>
                <given-names>Ann B.</given-names>
            </name>
            <name>
                <surname>Spross</surname>
                <given-names>Judith A.</given-names>
            </name>
		 <name>
                <surname>Hanson</surname>
                <given-names>Charlene M.</given-names>
            </name>
          </person-group>
           <source>Advanced practice nursing: an integrative approach</source>
 		<edition>3rd ed</edition>
  		<publisher-loc>St. Louis (MO)</publisher-loc>
 		<publisher-name>Elsevier Saunders</publisher-name>
<year>c2005</year>
 		<size units="page">979 p</size>
        </element-citation>
    </ref>
```

**Capítulo de livro:** **@publication-type="book"**

```
<ref id="B2">
        <label>2</label>
        <mixed-citation>Calkins BM, Mendeloff AI. The epidemiology of idiopathic inflammatory bowel disease. In: Kirsner JB, Shorter RG, eds. Inflammatory bowel disease, 4th ed. Baltimore: Williams &amp; Wilkins. 1995:31-68.</mixed-citation>
        <element-citation publication-type="book">
          <person-group person-group-type="author">
            <name>
                <surname>Calkins</surname>
                <given-names>BM</given-names>
            </name>
            <name>
                <surname>Mendeloff</surname>
                <given-names>AI</given-names>
            </name>
          </person-group>
            <part-title>The epidemiology of idiopathic inflammatory bowel
            disease.</part-title>
            <person-group person-group-type="editor">
                <name>
                    <surname>Kirsner</surname>
                    <given-names>JB</given-names>
                </name>
                <name>
                    <surname>Shorter</surname>
                    <given-names>RG</given-names>
                </name>
            </person-group>
            <source>Inflammatory bowel disease</source>
            <edition>4th</edition>
            <publisher-loc>Baltimore</publisher-loc>
            <publisher-name>Williams &amp; Wilkins</publisher-name>
            <year>1995</year>
            <fpage>31</fpage>
            <lpage>68</lpage>
        </element-citation>
    </ref>
```

**Dados de Pesquisa (dataset): @publication-type="data"**

```
<ref id="B2">
    <label>2</label>
<mixed-citation>Lucas Leão; Perobelli, Fernando Salgueiro; Ribeiro, Hilton Manoel Dias, 2024, Data for: Ação Coletiva Institucional e Consórcio Públicos Intermunicipais no Brasil, DOI: 10.48331/scielodata.5Z4TMP, SciELO Data, V1, UNF:6:Neyjad4du3rFprhupCXizA== [fileUNF]. Disponível em: https://doi.org/10.48331/scielodata</mixed-citation>
        <element-citation publication-type="data">
            <person-group person-group-type="author">
					<name>
						<surname>Leão</surname>
						<given-names>Lucas</given-names>
					</name>
					<name>
						<surname>Perobelli</surname>
						<given-names>Fernando Salgueiro</given-names>
					</name>
					<name>
						<surname>Ribeiro</surname>
						<given-names>Hilton Manoel Dias</given-names>
					</name>
				</person-group>
            <data-title>Data for: Ação Coletiva Institucional e Consórcio Públicos Intermunicipais no Brasil</data-title>
            <version>V1</version>
            <year>2024</year>
            <source>SciELO Data</source>
            <pub-id pub-id-type="art-access-id">UNF:6:Neyjad4du3rFprhupCXizA== [fileUNF]</pub-id>
            <pub-id pub-id-type="doi">10.1590/0123-45620187214</pub-id>
		 <ext-link ext-link-type="doi" xlink:href="https://doi.org/10.48331/scielodata">https://doi.org/10.48331/scielodata</ext-link>
        </element-citation>
</ref>
```

**Site: @publication-type="webpage"**

```
<ref id="B4">
    <label>4</label>
    <mixed-citation>COB - Comitê Olímpico Brasileiro. Desafio para o corpo. Disponível em: http://www.cob.org.br/esportes/esporte.asp?id=39. (Acesso em 10 abr 2010)</mixed-citation>
    <element-citation publication-type="webpage">
        <person-group person-group-type="author">
            <collab>COB -Comitê Olímpico Brasileiro</collab>
        </person-group>
        <source>Desafio para o corpo</source>
       <ext-link ext-link-type="uri" xlink:href="http://www.cob.org.br/esportes/esporte.asp?id=39">http://www.cob.org.br/esportes/esporte.asp?id=39</ext-link>
        <date-in-citation content-type="access-date">10 abr 2010</date-in-citation>
    </element-citation>
</ref>
```

**Proceedings: @publication-type="confproc"**

```
    <ref id="B6">
        <label>6</label>
        <mixed-citation>Furton EJ, Dort V, editors. Addiction and compulsive behaviors. Proceedings of the 17th Workshop for Bishops; 1999; Dallas, TX. Boston: National Catholic Bioethics Center (US); 2000. 258 p.</mixed-citation>
        <element-citation publication-type="confproc">
        	<person-group person-group-type="editor">
    			<name>
   				<surname>Furton</surname>
    				<given-names>EJ</given-names>
   			</name>
    			<name>
    				<surname>Dort</surname>
   				<given-names>V</given-names>
    			</name>
  		</person-group>
 	 	<source>Addiction and compulsive behaviors</source>
  		<conf-name>Proceedings of the 17th Workshop for Bishops</conf-name>
		<conf-num>17</conf-num>
  		<conf-date>1999</conf-date>
  		<conf-loc>Dallas, TX</conf-loc>
  		<publisher-loc>Boston</publisher-loc>
  		<publisher-name>National Catholic Bioethics Center (US)</publisher-name>
  		<year>2000</year>
  		<size units="page">258 p</size>
        </element-citation>
    </ref>
```

**Dissertação: @publication-type="thesis"**

```
    <ref id="B7">
        <label>7</label>
        <mixed-citation>Jones DL. The role of physical activity on the need for revision total knee arthroplasty in individuals with osteoarthritis of the knee [dissertation]. [Pittsburgh (PA)]: University of Pittsburgh; 2001. 436 p.</mixed-citation>
        <element-citation publication-type="thesis">
            <person-group person-group-type="author">
                <name>
    				<surname>Jones</surname>
    				<given-names>DL</given-names>
  			</name>
</person-group>
  		<source>The role of physical activity on the need for revision total knee arthroplasty in individuals with osteoarthritis of the knee [dissertation]</source>
  		<publisher-loc>[Pittsburgh (PA)]</publisher-loc>
  		<publisher-name>University of Pittsburgh</publisher-name>
  		<year>2001</year>
  		<size units="page">436 p</size>
        </element-citation>
    </ref>
```

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*:* *5.2.8.1. Textos em XML – SciELO Publishing Schema;* [Guia de citação de dados de pesquisa](https://wp.scielo.org/wp-content/uploads/guia-de-citacao-de-dados_pt.pdf)*;* [Sample PubMed Central Citations](https://www.ncbi.nlm.nih.gov/pmc/pmcdoc/tagging-guidelines/citations/v3/toc.html). |
| :---- |

| Consulte no SPS:  [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis); [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados); [Preprint: documentos publicados anteriormente como Preprint](#preprint:-documentos-publicados-anteriormente-como-preprint); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<ext-link\>](#\<ext-link\>:-link). |
| :---- |

## **\<related-article\>: Relação entre Documentos**  {#<related-article>:-relação-entre-documentos}

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Zero ou mais vezes |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Zero ou mais vezes |

Utilizado para indicar um documento relacionado ao que será publicado. Quando o documento relacionado também estiver publicado em SciELO deve-se adicionar o [\<related-article\>](#\<related-article\>:-relação-entre-documentos) neste documento.

Atributos obrigatórios:

1. @related-article-type  
2. @id  
3. @ext-link-type="doi"   
   1. Exceto para [parecer com link externo](#parecer:-revisão-por-pares-aberta) (@related-article-type="reviewer-report") e documento publicado anteriormente como [preprint](#preprint:-documentos-publicados-anteriormente-como-preprint) (@related-article-type="preprint") podendo ser utilizado o valor @ext-link-type="uri";  
   2. Dê preferência a "doi" se existir no documento.

Os valores possíveis para @related-article-type são:

| Valor | Descrição |
| :---: | ----- |
| **corrected-article** | Errata. |
| **correction-forward** | Documento corrigido pela errata. |
| **retracted-article** | Retratação total. |
| **retraction-forward** | Documento retratado totalmente. |
| **retracted-article** | Retratação parcial. |
| **partial-retraction** | Documento retratado parcialmente. |
| **addended-article** | Adendo. |
| **addendum** | Documento objeto do adendo. |
| **expression-of-concern** | Manifestação de preocupação. |
| **object-of-concern** | Documento objeto de manifestação de preocupação. |
| **commentary-article** | Comentário. |
| **commentary** | Documento comentado. |
| **reply** | Comentário objeto da resposta. |
| **commentary** | Resposta para um comentário. |
| **commentary-article** | Carta. |
| **letter** | Documento a que se refere a carta. |
| **reply** | Carta objeto da resposta. |
| **letter** | Resposta para uma carta. |
| **reviewed-article** | Parecer (revisão por pares). |
| **reviewer-report** | Documento com parecer (revisão por pares). |
|  **preprint** | Manuscrito disponibilizado em acesso aberto em um servidor web de preprints antes de ser publicado por um periódico. |

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplos:**

[**Preprint**](#preprint:-documentos-publicados-anteriormente-como-preprint) **relacionada a um documento:** @ext-link-type="uri"

```
<related-article related-article-type="preprint" id="r1" xlink:href="https://preprints.scielo.org/index.php/scielo/preprint/view/11166" ext-link-type="uri"/>
```

[**Errata**](#xml-da-errata) **relacionada a um documento:** @ext-link-type="doi"

```
<related-article related-article-type="corrected-article" id="r1" xlink:href="10.1590/123436773822" ext-link-type="doi"/>
```

[**Documento mencionado**](#xml-do-documento-mencionado-pela-errata) **pela errata:** @ext-link-type="doi"

```
<related-article related-article-type="correction-forward" id="r1" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
```

[**Resposta**](#xml-da-resposta-para-uma-carta) **para uma carta**

```
<related-article related-article-type="letter" id="r1" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
```

[**Carta relacionada**](#xml-da-carta) **a um documento e a [uma resposta](#xml-da-resposta-para-uma-carta)**

```
<related-article related-article-type="article" id="r1" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
<related-article related-article-type="reply" id="r2" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
```

| Consulte:  [Guia para Publicação de Errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf); [Guia para Publicação de Retratação](https://wp.scielo.org/wp-content/uploads/guia_retratacao.pdf); [Guia para Publicação de Adendo](https://wp.scielo.org/wp-content/uploads/guia_adendo.pdf); [Guia para publicação de Manifestação de Preocupação](https://wp.scielo.org/wp-content/uploads/guia_manifestacao.pdf). |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [Adendo](#adendo); [Carta](#carta); [Comentário](#comentário); [Errata](#errata); [Manifestação de Preocupação](#manifestação-de-preocupação); [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta); [Preprint: documentos publicados anteriormente como Preprint](#preprint:-documentos-publicados-anteriormente-como-preprint); [Retratação](#retratação). |
| :---- |

## **\<response\>: Conjunto de Respostas**  {#<response>:-conjunto-de-respostas}

| Aparece em | Ocorre |
| :---: | :---: |
| [\<article\>](#\<article\>:-artigo) | Zero ou mais vezes |
| [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Zero ou mais vezes |

Tag para identificar um **conjunto de respostas** referente a uma carta ou comentário. Obrigatoriamente publicadas juntamente a carta ou comentário. Se apenas **uma resposta** juntamente a carta ou comentário, usar [\<sub-article article-type="reply"\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo), se publicado com DOI e em separado da carta ou comentário usar [\<article article-type="reply"\>](#carta).

Atributos obrigatórios:

1. @response-type="reply"  
2. @xml:lang  
3. @id

**Exemplo:**

```
<response response-type="reply" xml:lang="en" id="S1">
 <front-stub>...</front-stub>
 <body>...</body>
<back>
   <ref-list>...</ref-list>
</back>
<response response-type="reply" xml:lang="en" id="S2">
 <front-stub>...</front-stub>
 <body>...</body>
<back>
   <ref-list>...</ref-list>
</back>
```

| Atenção:  Deve-se usar um @id diferente para cada [\<response\>](#\<response\>:-conjunto-de-respostas). |
| :---- |

| Consulte no SPS:  [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<article\>: Artigo](#\<article\>:-artigo); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [XML da Resposta para uma Carta](#xml-da-resposta-para-uma-carta). |
| :---- |

## **\<sec\>: Seção de Texto** {#<sec>:-seção-de-texto}

![Símbolo da Acessibilidade: Seções com marcação XML que possuem boas práticas de acessibilidade.][image3]Esta seção possui boas práticas de acessibilidade.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<abstract\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief) | Zero ou mais vezes |
| [\<app\>](#\<app-group\>:-apêndice-e-anexo) | Zero ou mais vezes |
| \<back\> | Zero ou mais vezes |
| [\<bio\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/bio.html) | Zero ou mais vezes |
| \<body\> | Zero ou mais vezes |
| [\<boxed-text\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/boxed-text.html) | Zero ou mais vezes |
| [\<sec\>](#\<sec\>:-seção-de-texto) | Zero ou mais vezes |
| [\<trans-abstract\>](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief) | Zero ou mais vezes |

* O texto do documento pode ser constituído por seções. Cada uma delas tendo **obrigatoriamente um elemento \<title\>**, seguido de um ou mais parágrafos \<p\>. Para criar títulos acessíveis, \<title\> deve ser incluído em cada [\<sec\>](#\<sec\>:-seção-de-texto).

Seções de primeiro nível que condizem com a lista de valores abaixo devem, obrigatoriamente, apresentar um atributo @sec-type. Caso haja seção de primeiro nível diferente do que consta na tabela, o referido atributo não deve ser inserido.

@sec-type="data-availability" (mais o atributo [@specific-use](#declaração-de-disponibilidade-de-dados)) é mandatório para os seguintes [tipos de documentos](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>):

1. data-article  
2. brief-report   
3. case-report  
4. rapid-communication  
5. research-article  
6. review-article

Os valores possíveis para @sec-type são:

| Valor | Descrição |
| :---: | ----- |
| **cases** | Relatos/casos/estudos de caso |
| **conclusions** | Conclusões/considerações finais/comentários |
| **data-availability** | Declaração de Disponibilidade de Dados (Ver mais em [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados)) |
| **discussion** | Discussões/interpretações |
| **intro** | Introdução/sinopse |
| **materials** | Materiais |
| **methods** | Metodologias/métodos/procedimentos |
| **results** | Resultados/descobertas |
| **subjects** | Participantes/Pacientes |
| **supplementary-material** | Material suplementar/material adicional (Ver mais em [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar)) |
| **transcript** | Transcrição de vídeo ou áudio (Ver mais em [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>)) |

No caso de seções combinadas, ou seja, quando o título for composto por mais de um desses itens (exceto para supplementary-material, transcript e data-availability), o valor do atributo @sec-type deverá corresponder a cada um, respectivamente, separados pelo caractere **|** (pipe). Exemplo: materials**|**methods.

**Exemplos:**

**Seção de texto:** Métodos

```
<sec sec-type="methods">
<title>Métodos de Pesquisa</title>
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent ornare magna a enim dapibus, sed tristique risus scelerisque. Mauris ultricies nunc sapien, ac iaculis risus pulvinar in.</p>
</sec>
```

**Seção de texto:** [Material Suplementar](#\<supplementary-material\>:-material-suplementar)

```
<sec sec-type="supplementary-material" id="sec1">
  <title>Supplementary Materials</title>
        <supplementary-material id="suppl1">
            <label>Supplementary material 1</label>
            <caption>
                <title>Video 1</title>
            </caption>
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4"/>
   </supplementary-material>
</sec>
```

**Seção de texto Combinada:** Materiais e Métodos (uso de pipe **|**)

```
<sec sec-type="materials|methods">
<title>Materials and Methods</title>
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent ornare magna a enim dapibus, sed tristique risus scelerisque. Mauris ultricies nunc sapien, ac iaculis risus pulvinar in.</p>   
</sec>
```

**Seção de texto:** [Transcrição de áudio ou vídeo](#\<sec-sec-type="transcript"\>) (Exige @id)

```
<sec sec-type="transcript" id="TR1">
<title>Interview with Gabriel and Denise</title>
<p>Nam convallis dolor sed ligula mollis vulputate. Mauris id felis id erat bibendum aliquam nec quis nulla. Sed nec augue orci. Donec rhoncus justo vitae enim finibus luctus. Praesent iaculis, velit iaculis efficitur accumsan, ex ligula elementum ipsum, et laoreet velit odio id nibh:</p>
<speech>
<speaker>Gabriel</speaker>
<p>Etiam ac arcu at nunc lacinia fermentum. Ut molestie vestibulum lacus, at ultricies orci ravida eget. Maecenas pellentesque leo ut sem cursus dictum. Nam at tempus arcu.</p>
</speech>
<speech>
<speaker>Denise</speaker>
<p>Pellentesque at bibendum nibh. Vestibulum non justo in nibh lobortis viverra eu eu magna. Etiam porta mollis libero, ut tempus est dictum eget. Vestibulum interdum leo vel dui malesuada, ac interdum arcu pharetra</p>
</speech>
<speech>
<speaker>Gabriel</speaker>
<p>Sed placerat dolor tellus</p>
</speech>
</sec>
```

**Seção de texto com subseção**

```
<sec sec-type="methods">
<title>Methodology</title>
<sec>
<title>Methodology in Science</title>
<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent ornare magna a enim dapibus, sed tristique risus scelerisque. Mauris ultricies nunc sapien, ac iaculis risus pulvinar in.</p> 
</sec>
</sec>
```

**Seção de texto sem tipologia definida**

```
<sec>
        <title>Biologia Marinha</title>
        <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi pharetra lacinia orci at adipiscing.</p>
<sec>
```

| Atenção:  \<title\> é mandatório para [\<sec\>](#\<sec\>:-seção-de-texto). |
| :---- |

| Consulte no SPS:  [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>); [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar); [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados); [Equivalência entre documentos indexáveis e @article-type em \<article\>](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>). |
| :---- |

## **\<sub-article\> \+ \<front-stub\>: Sub Artigo**  {#<sub-article>-+-<front-stub>:-sub-artigo}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| \<sub-article\> Aparece em | Ocorre |
| :---: | :---: |
| [\<article\>](#\<article\>:-artigo) | Zero ou mais vezes |
| [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Zero ou mais vezes |

Atributos obrigatórios em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo):

1. @article-type  
2. @id  
3. @xml:lang

Indica um documento aninhado dentro de outro. Obrigatório para documentos traduzidos.

Os sub artigos herdam os metadados do documento pai, sendo portanto necessário inserir um elemento [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) sem os elementos [\<journal-meta\>](#\<journal-meta\>:-metadados-do-periódico) e \<article-meta\>.

Com exceção do @article-type="translation" (um PDF para cada idioma), em todos os outros valores para @article-type os sub artigos devem estar descritos no mesmo PDF e **não podem ter a atribuição de DOI**.

Os valores possíveis para @article-type são:

| Valor | Descrição |
| :---: | ----- |
| **translation** | Tradução \- utilizado para o texto traduzido de um documento produzido em um idioma diferente. As traduções obrigatoriamente devem ser enviadas para publicação juntamente com sua versão no idioma original. Periódicos indexados no PMC e PubMed devem inserir como idioma original em [\<article\>](#\<article\>:-artigo) o texto em inglês (quando houver) e em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) os textos traduzidos em outros idiomas. |
| **reviewer-report** | [Parecer](#parecer:-revisão-por-pares-aberta) (revisão por pares) de documento. |
| **article-commentary** | [Comentário](#xml-do-comentário) para uma publicação. Em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) o documento e o comentário são publicados juntos no mesmo PDF. |
| **letter** | [Carta](#xml-da-carta) para uma publicação. Em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) o documento e a carta são publicadas juntas no mesmo PDF.  |
| **reply** | Resposta para uma [carta](#xml-da-resposta-para-uma-carta) ou [comentário](#xml-da-resposta-para-um-comentário). Em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) a resposta é publicada junto da carta ou comentário no mesmo PDF. |

Em [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo), obrigatoriamente devem ser inseridos os dados de [\<contrib-group\>](#\<contrib-group\>:-autoria) e [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento). Para  @article-type="translation" devem ser inseridas apenas as informações traduzidas em [\<aff\>](#\<aff\>:-afiliação-de-autores), quando houver a tradução do conteúdo textual. Caso não ocorra a tradução, use apenas a marcação \<institution content-type="original"\> com o idioma original da afiliação. O [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) deve possuir também a marcação de [resumo](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief), palavras-chaves e [notas de autor](#\<author-notes\>-+-\<fn\>:-notas-de-autor) traduzidas. Os dados de \<elocation-id\>, [DOI](#\<article-id\>:-doi-e-other) e [\<history\>](#\<history\>:-datas-de-histórico), quando diferentes do idioma original, também devem ser marcados em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo), caso contrário, esses elementos não devem se repetir em  [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). O idioma da seção obrigatoriamente deve acompanhar o idioma do texto em [\<article-categories\>](#\<article-categories\>:-seção-de-documento). Todos os outros dados textuais traduzidos devem ser marcados em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo): texto, notas, financiamento, etc.

Para publicação de Parecer, Carta, Comentário e Resposta como um documento PDF à parte do documento PDF referenciado e com número DOI próprio, consulte: [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta), [Carta](#carta), [Comentário](#comentário) e [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos).

**Exemplo:**

```
<sub-article article-type="translation" id="S1" xml:lang="pt">
		<front-stub>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>RELATO DE EXPERIÊNCIA</subject>
				</subj-group>
			</article-categories>
			<title-group>
				<article-title>Transferência tecnológica durante a pandemia de covid-19: relato do primeiro treinamento prático presencial no Brasil</article-title>
			</title-group>
			<contrib-group>
				<contrib contrib-type="author">
					<contrib-id contrib-id-type="orcid">0000-0002-0088-9036</contrib-id>
					<name>
						<surname>Carneiro</surname>
						<given-names>Tainá Ferreira</given-names>
					</name>
					<xref ref-type="aff" rid="aff9">1</xref>
					<role> concepção e delineamento </role>
					<role> análise e interpretação </role>
					<role> redação e revisão </role>
					<role> integridade </role>
				</contrib>	
            </contrib-group>
			<aff id="aff9">
				<label>1</label>
				<institution content-type="original">Universidade Federal da Bahia, Instituto Multidisciplinar em Saúde, Vitória da Conquista, BA, Brazil</institution>
			</aff>			
			<author-notes>				
				<fn fn-type="conflict" id="fn7">
					<p>CONFLITOS DE INTERESSE Os autores declararam não haver conflitos de interesse.</p>
				</fn>
			</author-notes>			
			<abstract>
				<title>Resumo</title>
				<p>O relato descreveu o primeiro curso presencial visando capacitar profissionais de saúde pública na realização de vigilância genômica em tempo real, durante períodos pandêmicos. Relato de experiência sobre um curso teórico-prático com foco em pesquisa e vigilância genômica, incluindo tecnologias de sequenciamento móvel, bioinformática, filogenética e modelagem epidemiológica. O evento contou com 162 participantes e foi o primeiro grande treinamento presencial realizado durante a epidemia de covid-19 no Brasil. Não foi detectada infecção pelo SARS-CoV-2 ao final do evento em nenhum participante, sugerindo a segurança e efetividade de todas as medidas de segurança adotadas. Os resultados do evento sugerem que é possível executar capacitação profissional com segurança durante pandemias, desde que seguidos todos os protocolos de segurança.</p>
			</abstract>
			<kwd-group xml:lang="pt">
				<title>Palavras-chave:</title>
				<kwd>Covid-19</kwd>
				<kwd>Pandemia</kwd>
				<kwd>Capacitação Profissional</kwd>
				<kwd>Capacitação de Recursos Humanos em Saúde.</kwd>
			</kwd-group>
		</front-stub>
  ...
  </sub-article>
```

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.3. Tipos de documentos*. |
| :---- |

| Consulte no SPS:  [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<article\>: Artigo](#\<article\>:-artigo); [DOCUMENTOS INDEXÁVEIS E NÃO INDEXÁVEIS](#🔹documentos-indexáveis-e-não-indexáveis):  [Documentos Indexáveis](#heading=h.7lpqbk8y8ugb); [Documentos Não Indexáveis](#documentos-não-indexáveis); [Equivalência entre documentos indexáveis e @article-type em \<article\>](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>). [\<aff\>: Afiliação de Autores](#\<aff\>:-afiliação-de-autores); [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<article-categories\>: Seção de Documento](#\<article-categories\>:-seção-de-documento); [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento); [\<abstract\>: Resumo, Highlights, Visual Abstract e In Brief](#\<abstract\>:-resumo,-highlights,-visual-abstract-e-in-brief); [\<author-notes\> \+ \<fn\>: Notas de Autor](#\<author-notes\>-+-\<fn\>:-notas-de-autor); [Parecer: Revisão por Pares Aberta](#parecer:-revisão-por-pares-aberta); [XML da Carta](#xml-da-carta); [XML do Comentário;](#xml-do-comentário) [XML da Resposta para uma Carta](#xml-da-resposta-para-uma-carta); [XML da Resposta para um Comentário](#xml-da-resposta-para-um-comentário); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

## **\<supplementary-material\>: Material Suplementar**  {#<supplementary-material>:-material-suplementar}

***e seção \<sec sec-type="supplementary-material\>*** 

| Aparece em | Ocorre |
| :---: | :---: |
| [\<sec\>](#\<sec\>:-seção-de-texto) | Zero ou mais vezes |
| \<article-meta\> | Zero ou mais vezes |

Atributo obrigatório em [\<sec\>](#\<sec\>:-seção-de-texto):

1. @sec-type="supplementary-material"

Atributo obrigatório em [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar):

1. @id 

Atributos obrigatórios em [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia):

1. @mime-type:   
2. @mime-subtype:  
3. @xlink:href

Para mais formatos de @mime-type e @mime-subtype consultar os valores possível em [Media Types;](https://www.iana.org/assignments/media-types/media-types.xhtml)

Atributo obrigatório em [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura):

1. @xlink:href

Material suplementar corresponde a todo conteúdo **enviado separadamente do PDF do documento** e que complementa o trabalho publicado, não estando incorporado ao corpo principal do texto.

No XML, o material suplementar deve ser representado por:

* Uma seção  [\<sec sec-type="supplementary-material"\>](#\<sec\>:-seção-de-texto) posicionada como última seção de \<body\> ou em \<back\> independentemente da posição em que o material é mencionado no PDF.   
  * [\<sec sec-type="supplementary-material"\>](#\<sec\>:-seção-de-texto) requer obrigatoriamente o elemento \<title\>  
* Um elemento [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar) para **cada item suplementar**.  
  * Cada [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar) requer obrigatoriamente um \<label\>   
  * Opcionalmente pode-se usar \<caption\> com \<title\>  
  * Não é permitido o uso de \<inline-supplementary-material\>  
  * Dentro de [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar) deve-se utilizar:  
    * [\<graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura) para figura  
    * [\<media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia) para outros tipos de conteúdo (PDF, word, excel, vídeo, entre outros)

É comum que materiais suplementares sejam mencionados apenas por meio de **links externos (URLs)**. Nestes casos, aplicam-se as seguintes regras:

* A existência de um link **não dispensa o envio do arquivo** junto ao pacote do documento;  
* Materiais suplementares referenciados por URL **devem ser enviados no pacote** e marcados normalmente como material suplementar: [\<sec sec-type="supplementary-material"\>](#\<sec\>:-seção-de-texto) e [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar);  
* O link externo pode ser mantido como informação adicional onde aparece no texto, marcado com \<ext-link\>, mas não substitui o envio do arquivo no fluxo editorial.

Essa prática está alinhada às recomendações do PMC/JATS, que determinam que o conteúdo suplementar referenciado esteja disponível de forma controlada no pacote editorial.

| Atenção:  Quando o link referenciar explicitamente um conjunto de dados de pesquisa (dataset),  não deve ser tratado como material suplementar; nestes casos, deve ser descrito na [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados); Quando o conteúdo, aparece integralmente no PDF não deve ser tratado como material suplementar: Se estiver em \<body\>: Deve ser marcado no local onde aparece, usando o elemento semântico correspondente (\<sec\>, \<fig\>, \<table-wrap\>, entre outros) Se estiver em \<back\>: deve ser tratado como um Anexo ou Apêndice marcado com [\<app-group](#\<app-group\>:-apêndice-e-anexo)\> e [\<app\>](#\<app-group\>:-apêndice-e-anexo) |
| :---- |

**Exemplos:** 

**Múltiplos materiais suplementares com arquivos incluídos no pacote**

```
<sec sec-type="supplementary-material" id="sec1">
  <title>Supplementary Materials</title>
        <supplementary-material id="suppl1">
            <label>Supplementary material 1</label>
            <caption>
                <title>Video 1</title>
            </caption>
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-m1.mp4"/>
</supplementary-material>
<supplementary-material id="suppl2">
<label>Supplementary material 2</label>
    		<caption>
       		 <title>Figure 1</title>
   		</caption>
   		<graphic xlink:href="1234-5678-scie-58-e1043-gf3.jpg"/>
 </supplementary-material>
 <supplementary-material id="suppl3">
            <label>Supplementary material 3</label>
             <caption>
                <title>Spreadsheet 1</title>
             </caption>
<media mimetype="application" mime-subtype="xlsx" xlink:href="1234-5678-scie-58-e1043-md2.xlsx"/>
 </supplementary-material>
 <supplementary-material id="suppl4">
            <label>Supplementary material 4</label>
            <media mimetype="application" mime-subtype="pdf" xlink:href="1234-5678-scie-58-e1043-md3.pdf"/>
 </supplementary-material>
</sec>
```

**Material suplementar referenciado por URL com arquivo incluído no pacote**

```
<sec sec-type="supplementary-material" id="sec1">
	<title>Supplementary Material</title>
	<p>Supplementary information are available  at <ext-link ext-link-type="uri" xlink:href="https://www.scielo.br/">https://www.scielo.br/</ext-link></p>
		<supplementary-material id="suppl1">
			<label>Supplementary PDF</label>
				<media mimetype="application" mime-subtype="pdf" xlink:href="1234-5678-scie-58-e1043-md1.pdf"/>
		</supplementary-material>			
</sec>
```

| Consulte:  [Media Types](https://www.iana.org/assignments/media-types/media-types.xhtml); [Recommended Practices for Online Supplemental Journal Article Materials \- January 2013](https://groups.niso.org/higherlogic/ws/public/download/10055); [Lista de repositórios para depósito de dados de pesquisa](https://wp.scielo.org/wp-content/uploads/Lista-de-Repositorios-Recomendados_pt.pdf); [SciELO Data: FAQ](https://www.scielo.org/pt/sobre-o-scielo/scielo-data-pt/faq/). |
| :---- |

| Consulte na JATS:  [\<supplementary-material\> Supplementary Material Metadata](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/supplementary-material.html). |
| :---- |

| Consulte no SPS:  [Declaração de Disponibilidade de Dados](#declaração-de-disponibilidade-de-dados); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [\<media\> e \<inline-media\>: Objeto Multimídia](#\<media\>-e-\<inline-media\>:-objeto-multimídia); [\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto); [Nomeação de Arquivos](#nomeação-de-arquivos); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada); [\<app-group\>: Apêndice e Anexo](#\<app-group\>:-apêndice-e-anexo). |
| :---- |

## **\<table-wrap\>: Tabela**  {#<table-wrap>:-tabela}

| Aparece em | Ocorre |
| :---: | :---: |
| [\<app\>](#\<app-group\>:-apêndice-e-anexo) | Zero ou mais vezes |
| \<body\> | Zero ou mais vezes |
| \<p\> | Zero ou mais vezes |
| [\<sec\>](#\<sec\>:-seção-de-texto) | Zero ou mais vezes |
| [\<glossary\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/glossary.html) | Zero ou mais vezes |

[\<table-wrap\>](#\<table-wrap\>:-tabela) especifica todas as partes de uma única tabela.

Atributo obrigatório:

1. @id

Tabelas obrigatoriamente devem possuir pelo menos \<label\> ou \<caption\> \+ \<title\>, na ausência de ambos os dados na tabela original, deve-se usar:

```
<table-wrap id="t1">
<caption>
<title/>
</caption>
<table>
...
</table>
```

* Para tabelas, usar codificação baseada em [NISO JATS table model](https://jats.nlm.nih.gov/archiving/tag-library/1.3/element/table.html) e [Table Formatting](http://jats.nlm.nih.gov/publishing/tag-library/1.0/n-unw2.html#pub-tag-table-format), com a adição das regras:  
  * O primeiro nível da estrutura não pode conter o elemento \<tr\>, ex.: //table/tr;  
  * Elemento [\<th\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/th.html) apenas como descendente de [\<thead\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/thead.html);  
  * Elemento [\<td\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/td.html) apenas como descendente de [\<tbody\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/tbody.html).  
    

| Atenção:  Tabelas devem aparecer no XML logo abaixo da primeira chamada do texto, independente de onde o dado esteja no PDF, no entanto, apenas quando identificadas fora de [\<app-group\>](#\<app-group\>:-apêndice-e-anexo) e [\<supplementary-material\>](#\<supplementary-material\>:-material-suplementar). |
| :---- |

| Consulte:  [Comunicado](http://us4.campaign-archive2.com/?u=f26dcf71797dd37381acb4aa5&id=0211ed957f&e=%5BUNIQID) sobre codificação de tabelas enviado em 09/12/2016. |
| :---- |

| Consulte na JATS:  [NISO JATS table model](https://jats.nlm.nih.gov/archiving/tag-library/1.3/element/table.html); [Table Formatting](http://jats.nlm.nih.gov/publishing/tag-library/1.0/n-unw2.html#pub-tag-table-format); [\<colgroup\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/colgroup.html); [\<col\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/col.html); [\<thead\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/thead.html); [\<th\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/th.html); [\<tbody\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/tbody.html); [\<td\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/td.html); [\<attrib\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/attrib.html). |
| :---- |

| Consulte no SPS:  [\<table-wrap-foot\> \+ \<fn\>: Notas de Tabela](#\<table-wrap-foot\>-+-\<fn\>:-notas-de-tabela); [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [Nomeação de Arquivos](#nomeação-de-arquivos). |
| :---- |

**Exemplos:**

**Tabela simples**

```
<table-wrap id="t1">
	<label>TABLE I</label>
	<caption>
		<title>Domains and signaling questions used to analyze the risk of bias in the included articles</title>
	</caption>
	<table>
		<colgroup>
			<col/>
			<col/>
			<col/>
			<col/>
		</colgroup>
		<tbody>
			<tr>
				<td align="center">Domain</td>
				<td align="center">Sand fly identification and processing</td>
				<td align="center">Sample quality</td>
				<td align="center">Methods for food source identification</td>
			</tr>
			<tr>
				<td align="center">Signaling questions (Yes/No/Unclear)</td>
				<td align="center">Was any taxonomic key used? Was the processing of sand flies carried out adequately?</td>
				<td align="center">Was the characterisation of blood feeding in females conducted properly? Was the dissection and the preservation of females carried out adequately?</td>
				<td align="center">Was the methodology used to identify the food source appropriate? Were appropriate controls used? Were cut off points predefined?</td>
			</tr>
			<tr>
				<td align="center">Description</td>
				<td align="center">The classification key used to identify sand flies was informed and how the insects were dissected, and stored to preserve it until processing was described</td>
				<td align="center">The feeding level of the females was evaluated,the dissection and preservation methods were presented</td>
				<td align="center">The methodology used was adequately described, controls were used, and definitions of positive results were presented</td>
			</tr>
			<tr>
				<td align="center">Risk of bias (High/Low/Unclear)</td>
				<td align="center">Could the identification and processing of sand flies have introduced bias?</td>
				<td align="center">Could the verification of blood feeding or its interpretation have introduced bias?</td>
				<td align="center">Could the conduct or interpretation of the food source have introduced bias?</td>
			</tr>
		</tbody>
	</table>
</table-wrap>
```

**Apresentação da [tabela](https://www.scielo.br/j/mioc/a/NjcgthVg4DCwnDczkszJBtN/?lang=en) simples:**

**![Apresentação na interface SciELO na página de artigo de uma tabela simples.O rótulo e título da tabela aparece na primeira linha em azul marinho e a tabela possui 4 colunas e 4 linhas com o texto cinza separado por linhas horizontais cinza.][image8]**

**Tabela colorida**

```
<table-wrap id="t1">
				<label>Tabela 1</label>
				<caption>
					<title>Perfil das mulheres vítimas de violência - Por faixa etária e tipo de crime</title>
				</caption>
				<table frame="hsides" rules="all">
					<tbody>
						<tr align="center" valign="top" style="background-color:#E2EFD9">
							<td style="background-color:#B4C6E7">Em 2018, Mulheres vítimas de:</td>
							<td>Violência sexual</td>
							<td>Violência física</td>
							<td>Violência psicológica</td>
							<td>Violência patrimonial</td>
							<td>Violência moral</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E7E6E6">
							<td>Faixa</td>
							<td>De 0 a 29 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E7E6E6">
							<td>%</td>
							<td>71,66</td>
							<td>43,73</td>
							<td>59,6</td>
							<td>60,6</td>
							<td>60,6</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E2EFD9">
							<td style="background-color:#B4C6E7">Em 2019, Mulheres vítimas de:</td>
							<td>Violência sexual</td>
							<td>Violência física</td>
							<td>Violência psicológica</td>
							<td>Violência patrimonial</td>
							<td>Violência moral</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E7E6E6">
							<td>Faixa</td>
							<td>De 0 a 29 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E7E6E6">
							<td>%</td>
							<td>65,43</td>
							<td>47,06</td>
							<td>59,7</td>
							<td>61,3</td>
							<td>61,7</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E2EFD9">
							<td style="background-color:#B4C6E7">Em 2020, Mulheres vítimas de:</td>
							<td>Violência sexual</td>
							<td>Violência física</td>
							<td>Violência psicológica</td>
							<td>Violência patrimonial</td>
							<td>Violência moral</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E7E6E6">
							<td>Faixa</td>
							<td>De 0 a 29 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
							<td>De 30 a 59 anos</td>
						</tr>
						<tr align="center" valign="top" style="background-color:#E7E6E6">
							<td>%</td>
							<td>80,4</td>
							<td>51,1</td>
							<td>60,9</td>
							<td>61,7</td>
							<td>62,8</td>
						</tr>
					</tbody>
				</table>
				<table-wrap-foot>
					<attrib>Fonte: Adaptado de Instituto de Segurança Pública do Rio de Janeiro ([2022]).<attrib>
				</table-wrap-foot>
</table-wrap>
```

**Apresentação da [tabela](https://www.scielo.br/j/mediacoes/a/rKSVCYvdbV6b4vV9tFxpzXz/?lang=pt) colorida:**

**![Apresentação na interface SciELO na página de artigo de uma tabela colorida.O rótulo e título da tabela aparece na primeira linha em azul marinho e a tabela possui 6 colunas e 9 linhas com o texto cinza separado por linhas horizontais cinza. As linhas 1, 3 e 6 possuem a primeira coluna com fundo azul e as 5 colunas seguintes com fundo verde. As linhas 2, 3, 5, 6, 8 e 9 possuem fundo cinza claro. A tabela também apresenta uma última linha com undo cinza claro com a informação da Fonte.][image9]**

**Tabela com célula mesclada**

```
<table-wrap id="t6" position="float">
<label>Table 6</label>
	<caption>
		<title>Mean (±SE) tetrazolium (TZ) vigor and viability of soybean seeds stored under different storage environments and bag depths. CV:coefficient of variation.</title>
	</caption>
	<table frame="hsides" rules="groups">
		<col width="43.24%"/>
		<col width="28.12%"/>
		<col width="28.64%"/>
		<thead>
			<tr>
				<th align="left" rowspan="2" scope="col" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle"/>
				<th align="center" scope="col" style="border-top: solid 0.50pt" valign="middle">TZ - Vigor</th>
				<th align="center" scope="col" style="border-top: solid 0.50pt" valign="middle">TZ - Viability</th>
			</tr>
			<tr>
				<th align="center" colspan="2" scope="colgroup" style="border-bottom: solid 0.50pt" valign="middle">-------------%--------------</th>
			</tr>
		</thead>
		<tbody>
			<tr>
				<td align="center" colspan="3" scope="col" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">Environment</td>
			</tr>
			<tr>
				<td align="justify" scope="row" style="border-top: solid 0.50pt" valign="middle">Chilled</td>
				<td align="center" style="border-top: solid 0.50pt" valign="middle">69.00±4.25a</td>
				<td align="center" style="border-top: solid 0.50pt" valign="middle">90.33±0.61a</td>
			</tr>
			<tr>
				<td align="justify" scope="row" valign="middle">With blanket</td>
				<td align="center" valign="middle">74.66±3.17a</td>
				<td align="center" valign="middle">90.33±1.89a</td>
			</tr>
			<tr>
				<td align="justify" scope="row" style="border-bottom: solid 0.50pt" valign="middle">Without blanket</td>
				<td align="center" style="border-bottom: solid 0.50pt" valign="middle">64.33±2.85a</td>
				<td align="center" style="border-bottom: solid 0.50pt" valign="middle">86.66±0.67a</td>
			</tr>
			<tr>
				<td align="center" colspan="3" scope="col" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">Bag depth</td>
			</tr>
			<tr>
				<td align="justify" scope="row" style="border-top: solid 0.50pt" valign="middle">Top</td>
				<td align="center" style="border-top: solid 0.50pt" valign="middle">68.33±3.63a</td>
				<td align="center" style="border-top: solid 0.50pt" valign="middle">88.66±1.12a</td>
			</tr>
			<tr>
				<td align="justify" scope="row" valign="middle">Middle</td>
				<td align="center" valign="middle">67.00±4.81a</td>
				<td align="center" valign="middle">90.33±1.41a</td>
			</tr>
			<tr>
				<td align="justify" scope="row" style="border-bottom: solid 0.50pt" valign="middle">Bottom</td>
				<td align="center" style="border-bottom: solid 0.50pt" valign="middle">72.66±2.67a</td>
				<td align="center" style="border-bottom: solid 0.50pt" valign="middle">88.33±1.58a</td>
			</tr>
			<tr>
				<td align="justify" scope="row" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">CV (%)</td>
				<td align="center" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">12.20</td>
				<td align="center" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">3.41</td>
			</tr>
			<tr>
				<td align="justify" scope="row" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">Means</td>
				<td align="center" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">69.33</td>
				<td align="center" style="border-top: solid 0.50pt; border-bottom: solid 0.50pt" valign="middle">89.11</td>
			</tr>
		</tbody>
	</table>
	<table-wrap-foot>
		<fn>
			<p>Means in the same column followed by the same lowercase letter, for each factor studied, do not differ from each other by the Tukey test at p &lt; 0.05. Data are expressed are means ± without blanket error of four replicates. </p>
		</fn>
	</table-wrap-foot>
</table-wrap>
```

**Apresentação da [tabela](https://www.scielo.br/j/bjb/a/qc9t6bYNRCZ996MjHTP369t/?lang=en) com célula mesclada:**

**![Apresentação na interface SciELO na página de artigo de uma tabela com célula mesclada.O rótulo e título da tabela aparece na primeira linha em azul marinho e a tabela possui 3 colunas e 12 linhas com o texto cinza separado por linhas horizontais cinza. A primeira linha representa cabeçalho ocupando as duas última colunas. A segundo linha também de cabeçalho ocupa apenas a segunda coluna. A quarta e sétima linha representam outros cabeçalhos com texto centralizado para as 3 colunas. A tabela também possui uma nota de rodapé com fundo cinza.][image10]**

**Tabela colorida e com célula mesclada**

```
<table-wrap id="t4">
<label>Table 4</label>
<caption>
<title>Risk factors for segmental resection and surgical complications.</title>
</caption>
<table cellspacing="5" cellpadding="5" frame="hsides" rules="groups">
<thead style="background-color:#d0907b;">
<tr>
<th align="left" valign="middle" rowspan="2">n (%)</th>
<th align="center" valign="middle" rowspan="2">Number of events</th>
<th align="center" valign="middle" colspan="2">Segmental resection 26 (34.2)</th>
<th align="center" valign="middle" colspan="2">No-colectomy surgery 50 (65.7)</th>
</tr>
<tr>
<th align="center" valign="middle">Univariate risk ratio (95%CI)</th>
<th align="center" valign="middle">p-value</th>
<th align="center" valign="middle">Multivariate risk ratio (95%CI)</th>
<th align="center" valign="middle">p-value</th>
</tr>
</thead>
<tbody style="background-color:#e8c8bc;">
<tr>
<td align="left" valign="middle" colspan="6">Surgical outcomes</td>
</tr>
<tr style="background-color:#f6e9e4;">
<td align="left" valign="middle">Major surgical complications (CD≥3)</td>
<td align="left" valign="middle">6</td>
<td align="left" valign="middle">1.92 (0.41–8.86)</td>
<td align="left" valign="middle">0.395</td>
<td align="left" valign="middle">1.95 (0.43–8.83)</td>
<td align="left" valign="middle">0.385</td>
</tr>
<tr>
<td align="left" valign="middle"> Reoperation</td>
<td align="left" valign="middle">2</td>
<td align="left" valign="middle">-</td>
<td align="left" valign="middle">-</td>
<td align="left" valign="middle">0.29 (0.12–0.70)</td>
<td align="left" valign="middle"><0.001</td>
</tr>
<tr style="background-color:#f6e9e4;">
<td align="left" valign="middle"> Concurrent hysterectomy</td>
<td align="left" valign="middle">31</td>
<td align="left" valign="middle">0.56 (0.28–1.12)</td>
<td align="left" valign="middle">0.076</td>
<td align="left" valign="middle">0.52 (0.07–3.47)</td>
<td align="left" valign="middle">0.500</td>
</tr>
<tr>
<td align="left" valign="middle" colspan="6">Predictors for segmental resection</td>
</tr>
<tr style="background-color:#f6e9e4;">
<td align="left" valign="middle"> Defecation pain</td>
<td align="left" valign="middle">11</td>
<td align="left" valign="middle">2.30 (0.77–6.85)</td>
<td align="left" valign="middle">0.124</td>
<td align="left" valign="middle">1.77 (1.20–3.39)</td>
<td align="left" valign="middle">0.014</td>
</tr>
<tr>
<td align="left" valign="middle"> Rectal bleed</td>
<td align="left" valign="middle">1</td>
<td align="left" valign="middle">-</td>
<td align="left" valign="middle">-</td>
<td align="left" valign="middle">3.00 (2.17–4.13)</td>
<td align="left" valign="middle">0.016</td>
</tr>
<tr style="background-color:#f6e9e4;">
<td align="left" valign="middle"> Constipation</td>
<td align="left" valign="middle">5</td>
<td align="left" valign="middle">1.28 (0.23–7.19)</td>
<td align="left" valign="middle">0.777</td>
<td align="left" valign="middle">0.62 (0.17–2.19)</td>
<td align="left" valign="middle">0.419</td>
</tr>
<tr>
<td align="left" valign="middle"> Any intestinal symptoms</td>
<td align="left" valign="middle">22</td>
<td align="left" valign="middle">1.33 (0.65–2.69)</td>
<td align="left" valign="middle">0.432</td>
<td align="left" valign="middle">1.29 (0.68–2.45)</td>
<td align="left" valign="middle">0.432</td>
</tr>
<tr style="background-color:#f6e9e4;">
<td align="left" valign="middle"> Uterine bleed</td>
<td align="left" valign="middle">13</td>
<td align="left" valign="middle">1.20 (0.43–3.30)</td>
<td align="left" valign="middle">0.722</td>
<td align="left" valign="middle">1.11 (0.39–3.13)</td>
<td align="left" valign="middle">0.843</td>
</tr>
<tr>
<td align="left" valign="middle"> Diarrhea</td>
<td align="left" valign="middle">8</td>
<td align="left" valign="middle">2.02 (1.06–3.05)</td>
<td align="left" valign="middle">0.047</td>
<td align="left" valign="middle">1.99 (1.03–3.82)</td>
<td align="left" valign="middle">0.038</td>
</tr>
</tbody>
</table>
<table-wrap-foot>
<fn>
<p>CI: confidence interval; CD: Clavien-Dindo classification.</p>
</fn>
</table-wrap-foot>
</table-wrap>
```

**Apresentação da [tabela](https://www.scielo.br/j/abcd/a/n9QGkSrjqTTyY4t5qJgJjBz/?lang=en) colorida e com célula mesclada:**

![Apresentação na interface SciELO na página de artigo de uma tabela colorida com célula mesclada.O rótulo e título da tabela aparece na primeira linha em azul marinho e a tabela possui 6 colunas e 12 linhas com o texto cinza separado por linhas horizontais cinza. A linha cabeçalho possui fundo marrom alaranjado escuro e as demais linhas alternam entre marrom médio e marrom claro. O cabeçalho possui informação textual nas duas primeira colunas e colunas 3, 4, 5 e 6 separam-se em duas linhas sendo a primeira linha separada por dois títulos e a segunda 4 títulos.][image11]

Para identificar fonte ou outros dados semelhantes que não representam notas de rodapé da tabela [\<table-wrap-foot\> \+ \<fn\>: Notas de Tabela](#\<table-wrap-foot\>-+-\<fn\>:-notas-de-tabela), use [\<attrib\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/attrib.html), exemplo:

```
<table-wrap-foot>
<attrib>Nota: Elaborado com base no Censo Escolar de 2023 (Inep, 2024).</attrib>
</table-wrap-foot>
```

## 

## **\<title-group\> e \<trans-title-group\>: Título de Documento** {#<title-group>-e-<trans-title-group>:-título-de-documento}

***Título \<article-title\> e Título Traduzido \<trans-title\>***   
[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| \<article-meta\> | Uma vez |
| [\<front-stub\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) | Uma vez |

Atributo obrigatório em [\<trans-title-group\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento):

1. @xml:lang

Utilizado para identificar o título do documento, título traduzido ou um conjunto de títulos traduzidos do documento. O título no idioma original utiliza o idioma descrito no atributo @xml:lang disponível em [\<article\>](#\<article\>:-artigo), por isso é mandatório o uso do @xml:lang apenas nos títulos traduzidos que serão marcados em [\<trans-title-group\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento). 

A indicação de título é mandatória para todas as coleções e a indicação de título diferente do título da seção é mandatória para a publicação na coleção SciELO Brasil, por exemplo, um documento com a seção Editorial, não deve ter como título a palavra Editorial. Para a errata, retratação, adendo e manifestação de preocupação, recomenda-se que o título do documento seja o mesmo termo da seção com adição de dois pontos mais o título do documento que sofre a errata, retratação, adendo e manifestação de preocupação.

**Exemplo de Editorial com título diferente da seção ([PDF](https://www.scielo.br/j/mioc/a/rqgVdMnwsKtPCFrFW6MV8wJ/?format=pdf&lang=en))**

```
<article-categories>
    <subj-group subj-group-type="heading">
        <subject>Editorial</subject>
    </subj-group>
</article-categories>
...
<title-group>
    <article-title>Article series: from the first issue of Mem Inst Oswaldo Cruz (1909) to the present (2024)</article-title>    
</title-group>
```

**Exemplo de título no idioma original [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) e títulos traduzidos [\<trans-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento):**

```
<title-group>
    <article-title>Between spiritual wellbeing and spiritual distress: possible related factors in elderly patients with cancer</article-title>
    <trans-title-group xml:lang="pt">
        <trans-title>Entre o bem-estar espiritual e a angústia espiritual: possíveis fatores relacionados a idosos com cancro</trans-title>
    </trans-title-group>
    <trans-title-group xml:lang="es">
        <trans-title>Entre el bienestar espiritual y el sufrimiento espiritual: posibles factores relacionados en ancianos con câncer</trans-title>
    </trans-title-group>
</title-group>
```

| Atenção:  Títulos que contenham um subtítulo devem ser marcados apenas nas tags de [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) e [\<trans-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) com separadores como dois pontos, traço, etc. Não use a marcação de subtítulos em outras tags. |
| :---- |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf): *5.2.8.1. Textos em XML – SciELO Publishing Schema*. |
| :---- |

| Consulte no SPS:  [\<article\>: Artigo](#\<article\>:-artigo). |
| :---- |

## **\<xref\>: Referência Cruzada**  {#<xref>:-referência-cruzada}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

| Aparece em | Ocorre |
| :---: | :---: |
| [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) | Zero ou mais vezes |
| [\<attrib\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/attrib.html) | Zero ou mais vezes |
| [\<contrib\>](#\<contrib\>:-\<name\>-e-\<collab\>) | Zero ou mais vezes |
| \<p\> | Zero ou mais vezes |
| \<td\> | Zero ou mais vezes |
| \<th\> | Zero ou mais vezes |
| [\<trans-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) | Zero ou mais vezes |
| [\<sec\>](#\<sec\>:-seção-de-texto) | Zero ou mais vezes |
| \<verse-line\> | Zero ou mais vezes |

Elemento de referência cruzada usado para relacionar alguma informação no texto.

A [\<xref\>](#\<xref\>:-referência-cruzada) com atributo e valor @ref-type="bibr" obrigatoriamente deve ocorrer pelo menos uma vez no documento para a coleção SciELO Brasil. 

Atributos obrigatórios:

1. @rid: Contém o identificador do elemento do documento referenciado, perfazendo assim o link entre a origem (@rid) e o destino (@id) no texto.  
2. @ref-type: Especifica o tipo de referência cruzada.

Os valores para @ref-type podem ser: 

| Valor | Descrição |
| :---: | ----- |
| **aff** | Afiliação |
| **app** | Apêndice |
| **author-notes** | Notas relacionadas ao autor |
| **bibr** | Referência bibliográfica |
| **bio** | Bibliografia do autor |
| **boxed-text** | Caixa de texto |
| **contrib** | Autoria |
| **corresp** | Autor correspondente |
| **disp-formula** | Fórmula/Equação |
| **fig** | Figura ou grupo de figuras |
| **fn** | Nota |
| **list** | Lista ou item da lista |
| **sec** | Seção |
| **supplementary-material** | Material suplementar |
| **table** | Tabela ou grupo de tabelas |
| **table-fn** | Nota de rodapé de tabelas |

\<xref ref-type=”sec”\> \+ @rid: é obrigatório quando existir a seção: [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>).

**Exemplos:**

[**\<xref\>**](#\<xref\>:-referência-cruzada)**:** para [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>)

```
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<label>Interview with Gabriel and Denise</label> 		
<long-desc>Descrição detalhada do objeto (acima de 120 caracteres)</long-desc>
<xref ref-type="sec" rid="TR1"/>
</media>
```

[**\<xref\>**](#\<xref\>:-referência-cruzada)**:** para [afiliação](#\<aff\>:-afiliação-de-autores) 

```
<xref ref-type="aff" rid="aff1">1</xref>
```

[**\<xref\>**](#\<xref\>:-referência-cruzada)**:** para [afiliação](#\<aff\>:-afiliação-de-autores) sem identificação de etiqueta no PDF

```
<xref ref-type="aff" rid="aff1"/>
```

[**\<xref\>**](#\<xref\>:-referência-cruzada)**:** para [figura](#\<fig\>:-figura)

```
<xref ref-type="fig" rid="f1">Figure 1</xref>
```

[**\<xref\>**](#\<xref\>:-referência-cruzada)**:** para [referência bibliográfica](#\<ref-list\>:-lista-de-referências) 

**autor-data:** John 2003

```
<xref ref-type="bibr" rid="B13">John 2003</xref>
```

**autor-data com intervalo de ano:** John (2003-2006)

```
<xref ref-type="bibr" rid="B13">John (2003-2006)</xref>
```

**numérica:** 1

```
<xref ref-type="bibr" rid="B1">1</xref>
```

**numérica sobrescrito:** 1

```
<xref ref-type="bibr" rid="B1"><sup>1</sup></xref>
```

**numérica sobrescrito com intervalo de citação:** (1-7)

```
<sup>(<xref ref-type="bibr" rid="B1">1</xref> - <xref ref-type="bibr" rid="B7">7</xref>)</sup>
```

| Atenção:  \<sup\> não pode abarcar [\<xref\>](#\<xref\>:-referência-cruzada) quando não há caracteres textuais, neste caso o \<sup\> deve estar dentro de [\<xref\>](#\<xref\>:-referência-cruzada); Todo @rid obrigatoriamente deve ter um @id correspondente no XML; Um @id pode ou não ter um @rid correspondente no XML;  É obrigatório a inserção de [\<xref\>](#\<xref\>:-referência-cruzada) fechada quando não há identificação de etiquetas \<label\> no PDF para [afiliação de autor](#\<aff\>:-afiliação-de-autores) @ref-type="aff", exemplo: \<xref ref-type="aff" rid="aff1"/\>; Para os outros valores obrigatoriamente deve ocorrer a menção ou etiqueta correspondente no texto. |
| :---- |

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.8.1. Textos em XML – SciELO Publishing Schema*. |
| :---- |

| Consulte no SPS:  [Sugestão de Atribuição de @id](#sugestão-de-atribuição-de-@id); [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis); [\<aff\>: Afiliação de Autores](#\<aff\>:-afiliação-de-autores);  [\<sec sec-type="transcript"\>](#\<sec-sec-type="transcript"\>) |
| :---- |

\----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

# **🔹LISTA DE MARCAÇÕES ESPECÍFICAS** {#🔹lista-de-marcações-específicas}

## **Adendo** {#adendo}

Adendo para documento publicado.

| Consulte:  [Guia para Publicação de Adendo](https://wp.scielo.org/wp-content/uploads/guia_adendo.pdf); [SciELO Ética](https://www.scielo.org/pt/sobre-o-scielo/scielo-etica/). |
| :---- |

### **XML do Adendo** {#xml-do-adendo}

O XML do adendo deve possuir a tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. Em [\<article\>](#\<article\>:-artigo) @article-type="addendum"  
2. [\<article-id pub-id-type="doi"\>](#\<article-id\>:-doi-e-other) com o DOI do adendo  
3. [\<subject\>](#\<article-categories\>:-seção-de-documento) com a mesma seção do PDF do adendo  
4. [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) com o mesmo título do PDF do adendo  
5. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   1. @related-article-type="addended-article"   
   2. @id  
   3. @xlink:href com número DOI do documento mencionado pelo adendo  
   4. ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="addendum" xml:lang="pt">
	...
	<front>
		<article-meta>
			<article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>Adendo</subject>
				</subj-group>
				...
			</article-categories>
			<title-group>
				<article-title>Adendo: Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
			</title-group>
			...
			<permissions>
				...
			</permissions>
			<related-article related-article-type="addended-article" id="r1" xlink:href="10.1590/abd1806-4841.20142998" ext-link-type="doi"/>
			<counts>
				...
		</article-meta>
		...
		</front>
           <body>
			<p>Texto do Adendo</p>
		</body>
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<article-categories\>: Seção de Documento](#\<article-categories\>:-seção-de-documento); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

### **XML do Documento Mencionado pelo Adendo** {#xml-do-documento-mencionado-pelo-adendo}

O XML do(s) documento(s) mencionado(s) pelo adendo deve ter a adição da tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="addendum"   
   * @id  
   * @xlink:href com número DOI do adendo  
   * ext-link-type="doi" 

Deve-se adicionar ainda em \<front\>, dentro de [\<history\>](#\<history\>:-datas-de-histórico), a data de aprovação do adendo do documento com:

1.  @date-type="corrected"

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\> Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="pt">
	...
	<front>
		<article-meta>
			<article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>Artigo Original</subject>
				</subj-group>
				...
			</article-categories>
			<title-group>
				<article-title>Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
			</title-group>			
			   ...
               <history>
				<date date-type="received">
			        ...
				<date date-type="accepted">
			        ...
				<date date-type="corrected">
					<day>01</day>
					<month>12</month>
					<year>2025</year>
				</date>
			</history>
			<permissions>
			</permissions>
              ...
            <related-article related-article-type="addendum" id="r1" xlink:href="10.1590/123456720182998ad" ext-link-type="doi"/>
             <counts>							
          </article-meta>
		...
	 </front>
	 <body>
		...
	 </body>
	 <back>
		...
	</back>
</article>
```

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html). |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

## **Carta** {#carta}

Carta para uma publicação. Se publicada separadamente ao documento referido ([\<article\>](#\<article\>:-artigo)), seguir a marcação informada abaixo. Deve possuir os [6 itens mandatórios para publicação](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis). Para publicação no mesmo PDF do documento a marcação deve ocorrer em  [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo).

Para a publicação de Carta e Resposta como [\<article\>](#\<article\>:-artigo), é necessária a entrega do pacote de submissão, conforme descrito a seguir.

**Pacote Carta**

Deve conter:

1. XML da Carta  
2. PDF da Carta  
3. XML do documento mencionado pela Carta, com a adição do elemento [\<related-article\>](#\<related-article\>:-relação-entre-documentos)  
   1. A entrega do PDF do documento mencionado pela Carta é opcional.  
   2. Caso o periódico tenha incluído no PDF uma nota informando que o documento foi mencionado por uma carta, a entrega do PDF é obrigatória. Caso contrário, apenas o XML é requerido.

**Pacote Resposta**

Deve conter:

1. XML da Resposta  
2. PDF da Resposta  
3. XML da Carta, com a adição do elemento [\<related-article\>](#\<related-article\>:-relação-entre-documentos)  
   1. A entrega do PDF da Carta é opcional.  
   2. Caso o periódico tenha incluído no PDF uma nota informando que a carta recebeu uma resposta, a entrega do PDF é obrigatória. Caso contrário, apenas o XML é requerido.  
      

| Consulte no SPS:  [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis). |
| :---- |

      

### **XML da Carta** {#xml-da-carta}

1. Em [\<article\>](#\<article\>:-artigo) @article-type="letter"  
2. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="article"   
   * @id  
   * @xlink:href com número DOI do documento que está referido na carta  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="letter" xml:lang="pt">
    <front>
        ...
        <article-meta>
        ...
            </permissions>
            <related-article related-article-type="article" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>
            ...
        </article-meta>
    ...
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

### **XML da Carta com Resposta** {#xml-da-carta-com-resposta}

Neste caso, além do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) já existente para o documento referenciado pela Carta, adiciona-se um novo [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com outro @id  para a Resposta.

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="reply"   
   * @id  
   * @xlink:href com número DOI da resposta  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="letter" xml:lang="pt">
    <front>
        ...
        <article-meta>
        ...
            </permissions>
            <related-article related-article-type="article" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>
<related-article related-article-type="reply" id="r2" xlink:href="10.1590/123456720185471" ext-link-type="doi"/>
            ...
        </article-meta>
    ...
</article>  
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

### **XML do Documento Mencionado pela Carta** {#xml-do-documento-mencionado-pela-carta}

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="letter"   
   * @id  
   * @xlink:href com número DOI do comentário  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
  </permissions>
            <related-article related-article-type="letter" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>     
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

### **XML da Resposta para uma Carta**  {#xml-da-resposta-para-uma-carta}

Se publicado juntamente com a carta (mesmo PDF):

* Quando tiver apenas uma resposta marcar como [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo);  
* Se corresponder a um conjunto de respostas (duas ou mais) marcar como [\<response\>](#\<response\>:-conjunto-de-respostas). 

Para publicação da resposta em outro PDF, separado da carta, considerar:

1. Em [\<article\>](#\<article\>:-artigo) @article-type="reply"  
2. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="letter"   
   * @id  
   * @xlink:href com número DOI da carta  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="reply" xml:lang="pt">
    <front>
        ...
        <article-meta>
        ...
            </permissions>
            <related-article related-article-type="letter" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>
            ...
        </article-meta>
    ...
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [\<response\>: Conjunto de Respostas](#\<response\>:-conjunto-de-respostas). |
| :---- |

## **Comentário**  {#comentário}

Comentário para uma publicação. Se publicado separadamente ao documento referido ([\<article\>](#\<article\>:-artigo)), seguir a marcação informada abaixo. Deve possuir os [6 itens mandatórios para publicação](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis). Para publicação no mesmo PDF do documento a marcação deve ocorrer em  [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo).

Para a publicação de Comentário e Resposta como [\<article\>](#\<article\>:-artigo), é necessária a entrega do pacote de submissão, conforme descrito a seguir.

**Pacote Comentário**

Deve conter:

1. XML do Comentário  
2. PDF do Comentário  
3. XML do documento comentado, com a adição do elemento [\<related-article\>](#\<related-article\>:-relação-entre-documentos)  
   1. A entrega do PDF do documento comentado é opcional.  
   2. Caso o periódico tenha incluído no PDF uma nota informando que o documento recebeu um comentário, a entrega do PDF é obrigatória. Caso contrário, apenas o XML é requerido.

**Pacote Resposta**

Deve conter:

1. XML da Resposta  
2. PDF da Resposta  
3. XML do Comentário, com a adição do elemento [\<related-article\>](#\<related-article\>:-relação-entre-documentos)  
   1. A entrega do PDF do Comentário é opcional.  
   2. Caso o periódico tenha incluído no PDF uma nota informando que o comentário recebeu uma resposta, a entrega do PDF é obrigatória. Caso contrário, apenas o XML é requerido.  
      

| Consulte no SPS:  [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [ELEMENTOS OBRIGATÓRIOS PARA PUBLICAÇÃO DE DOCUMENTOS INDEXÁVEIS](#🔹elementos-obrigatórios-para-publicação-de-documentos-indexáveis). |
| :---- |

      

### **XML do Comentário** {#xml-do-comentário}

1. Em [\<article\>](#\<article\>:-artigo) @article-type="article-commentary"  
2. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="commentary-article"   
   * @id  
   * @xlink:href com número DOI do documento que está sendo comentado  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="article-commentary" xml:lang="pt">
    <front>
        ...
        <article-meta>
        ...
            </permissions>
            <related-article related-article-type="commentary-article" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>
            ...
        </article-meta>
    ...
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

### **XML do Comentário com Resposta** {#xml-do-comentário-com-resposta}

Neste caso, além do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) já existente para o artigo comentado adiciona-se um novo [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com outro @id para a resposta.

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="reply"   
   * @id  
   * @xlink:href com número DOI da resposta  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="article-commentary" xml:lang="pt">
    <front>
        ...
        <article-meta>
        ...
            </permissions>
            <related-article related-article-type="commentary-article" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>
 <related-article related-article-type="reply" id="r2" xlink:href="10.1590/123456720185471" ext-link-type="doi"/>
            ...
        </article-meta>
    ...
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

### **XML do Documento Comentado** {#xml-do-documento-comentado}

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="commentary"   
   * @id  
   * @xlink:href com número DOI do comentário  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
  </permissions>
            <related-article related-article-type="commentary" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

### **XML da Resposta para um Comentário**  {#xml-da-resposta-para-um-comentário}

Se publicado juntamente com o comentário (mesmo PDF):

* Quando tiver apenas uma resposta marcar como [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo);  
* Se corresponder a um conjunto de respostas (duas ou mais) marcar como [\<response\>](#\<response\>:-conjunto-de-respostas). 

Para publicação da resposta em outro PDF, separado do comentário, considerar:

1. Em [\<article\>](#\<article\>:-artigo) @article-type="reply"  
2. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="commentary"   
   * @id  
   * @xlink:href com número DOI do comentário  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="reply" xml:lang="pt">
    <front>
        ...
        <article-meta>
        ...
            </permissions>
            <related-article related-article-type="commentary" id="r1" xlink:href="10.1590/123456720182998" ext-link-type="doi"/>
            ...
        </article-meta>
    ...
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); [\<response\>: Conjunto de Respostas](#\<response\>:-conjunto-de-respostas). |
| :---- |

## **Declaração de Disponibilidade de Dados** {#declaração-de-disponibilidade-de-dados}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Um conjunto de dados, no contexto da marcação e publicação de documentos, é uma coleção estruturada de dados que estão associados a uma pesquisa e representam todo tipo de dado que subsidie o documento submetido ou aprovado para publicação e documentações que facilitem avaliação da pesquisa, reprodução dos resultados e reutilização dos dados de pesquisa.

Dada a importância crescente dos dados de pesquisa e seu reconhecimento como contribuição intelectual original, também os conjuntos de dados (datasets) subjacentes aos documentos devem ser mencionados nos documentos publicados. A declaração de disponibilidade de dados, nos documentos publicados em SciELO Brasil, deve ser uma prática para todos os periódicos indexados na coleção, sendo **obrigatório** o uso da declaração para os [tipos de documentos](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>) com valores de @article-type iguais a:

1. data-article  
2. brief-report   
3. case-report  
4. rapid-communication  
5. research-article  
6. review-article

Os demais [tipos de documentos](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>) indexáveis, \- exceto Errata (correction), Retratação (retraction e partial-retraction), Adendo (addendum) e Manifestação de Preocupação (expression-of-concern) \- opcionalmente podem conter a declaração de disponibilidade de dados.

A terminologia adotada para a denominação da seção ou nota, fica a critério do periódico. Os termos mais comuns utilizados são:

| Português | Inglês |
| ----- | ----- |
| Declaração de Disponibilidade de Dados ***(mais usado)*** | Data Availability Statement ***(mais usado)*** |
| Declaração de Disponibilidade de Dados de Pesquisa | Research Data Availability Statement  |
| Declaração sobre disponibilidade de dados | Statement about Data Availability |
| Disponibilidade dos Dados | Data Availability |
| Disponibilidade de Dados de Pesquisa | Research Data Availability |

Recomenda-se fortemente que os periódicos **não denominem este tipo de dado como material suplementar**, uma vez que este tipo de material não é essencial para a compreensão, reprodução e reutilização do trabalho. Para mais informações, consulte [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar).

A Declaração de Disponibilidade pode ser marcada de duas formas, como uma seção [\<sec\>](#\<sec\>:-seção-de-texto) de \<body\> ou \<back\> ou como uma nota [\<fn\> em \<fn-group\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento) de \<back\>. Em ambos os casos a indicação do título da seção/nota é mandatória, sendo:

* **Para [\<fn\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento):** \<label\>  
* **Para [\<sec\>](#\<sec\>:-seção-de-texto):** \<title\>

Os atributos obrigatórios para [\<fn\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento) são:

* @fn-type\="data-availability"  
* @specific-use

Os atributos obrigatórios para [\<sec\>](#\<sec\>:-seção-de-texto) são:

* @sec-type\="data-availability"  
* @specific-use

Os valores utilizados para @specific-use podem ser:

| Valor | Descrição |
| :---: | ----- |
| **data-available** | Os dados de pesquisa estão disponíveis em repositório. |
| **data-available-upon-request** | Os dados de pesquisa só estão disponíveis mediante solicitação. |
| **data-in-article** | Os dados de pesquisa estão disponíveis no corpo do documento. |
| **data-not-available** | Os dados de pesquisa não estão disponíveis. |
| **uninformed** | Uso de dados não informado; nenhum dado de pesquisa gerado ou utilizado. |

**Exemplos:**

**Marcação XML em \<back\> como nota [\<fn\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento)**

```
<back>
…
<fn-group>
 <fn fn-type="data-availability" specific-use="data-available" id="fn1">
 <label>Data Availability Statement</label>
 <p> xxxxxxxxxxxxxxxxxxxxxxx</p>
 </fn>
</fn-group>
```

**Marcação XML em \<body\> ou \<back\> como seção [\<sec\>](#\<sec\>:-seção-de-texto)**

```
<sec sec-type="data-availability" specific-use="data-available-upon-request">
<title>Disponibilidade de Dados</title>
<p>xxxxxxxxxxxxxxxxxxxxxxx</p>
</sec>
```

A nota ou seção pode conter link ou referência ao conteúdo do documento, tais como como tabelas, figuras, referências, etc., nestes casos use [\<ext-link\>](#\<ext-link\>:-link) e [\<xref\>](#\<xref\>:-referência-cruzada) e outras tags pertinentes quando necessário. 

Recomenda-se que, quando o valor usado for @specific-use="data-in-article" (os dados estão no documento), os autores referenciem em quais partes do documento estão os dados de pesquisa, usando-se de referência cruzada [\<xref\>](#\<xref\>:-referência-cruzada). **Exemplo:**

```
<fn-group>
<fn fn-type="data-availability" specific-use="data-in-article" id="fn1">
 		<label>Disponibilidade de Dados</label>
 			<p>Os dados de pesquisa estão disponíveis no artigo. Consulte: <xref ref-type="bibr" rid="B22">Thompson et al. 2004</xref>, <xref ref-type="table" rid="t3">Tabela 3</xref>, <xref ref-type="fig" rid="f4">figura 4</xref> e <xref ref-type="fig" rid="f7">figura 7</xref></p>
</fn>
</fn-group>
```

Se os dados de pesquisa não estiverem depositados em um repositório de dados onde o link possa ser fornecido e também não comporem partes do documento que possam ser referenciadas, os dados devem ser enviados como objetos externos junto do pacote do documento e marcados com [\<media\>, \<inline-media\>](#\<media\>-e-\<inline-media\>:-objeto-multimídia), [\<graphic\> ou \<inline-graphic\>](#\<graphic\>-e-\<inline-graphic\>:-figura). Exemplo:

```
<sec sec-type="data-availability" specific-use="data-available">
  <title>Data Availability Statement</title>
<media mimetype="video" mime-subtype="mp4" xlink:href="1234-5678-scie-58-e1043-md1.mp4">
<label>Data 1</label>
 		<caption>
 			 <title>Vídeo</title> 
 </caption> 
</media>
 	 <fig>      
<label>Data 2</label>
    		<caption>
       		 <title>Figure</title>
   		</caption>
   		<graphic xlink:href="1234-5678-scie-58-e1043-gf3.jpg"/>
 	 </fig> 
<media mimetype="application" mime-subtype="xlsx" xlink:href="1234-5678-scie-58-e1043-md2.xlsx">
            <label>Data 3</label>
             <caption>
                <title>Spreadsheet</title>
             </caption>
</media>
 <media mimetype="application" mime-subtype="pdf" xlink:href="1234-5678-scie-58-e1043-md3.pdf">
             <label>Data 4</label>
            <caption>
                <title>Document</title>
             </caption>
</media>
</sec>
```

Em adição a nota ou seção da Declaração de Disponibilidade de Dados é **altamente recomendado** que os dados de pesquisa sejam citados na lista de referências do documento. Para mais informações consulte [Guia de citação de dados de pesquisa](https://wp.scielo.org/wp-content/uploads/guia-de-citacao-de-dados_pt.pdf). **Exemplo** de referência para um Dataset:

```
<ref-list>
...
<ref id="B2">
    <label>2</label>
 	  <mixed-citation>Fontoura, Larissa Casaril da; Urbano, Mariana Ragassi; Kanashiro, Milena, 2025, "Data for: Comparing land use mix measures for different urban areas: an application in a Brazilian city", DOI: 10.48331/scielodata.UAAZRJ, SciELO Data, V1. Disponível em: https://data.scielo.org/dataset.xhtml?persistentId=doi:10.48331/scielodata.UAAZRJ
  </mixed-citation>
        <element-citation publication-type="data">
           <person-group person-group-type="author">
					<name>
						<surname>Fontoura</surname>
						<given-names>Larissa Casaril da</given-names>
					</name>
					<name>
						<surname>Urbano</surname>
						<given-names>Mariana Ragassi</given-names>
					</name>
					<name>
						<surname>Kanashiro</surname>
						<given-names>Milena</given-names>
					</name>
				</person-group>
            <data-title>Data for: Comparing land use mix measures for different urban areas: an application in a Brazilian city</data-title>
            <version>V1</version>
            <year>2025</year>
            <source>SciELO data</source>            
            <pub-id pub-id-type="doi">10.48331/scielodata.UAAZRJ</pub-id>
 <ext-link ext-link-type="uri" xlink:href="https://data.scielo.org/dataset.xhtml?persistentId=doi:10.48331/scielodata.UAAZRJ">https://data.scielo.org/dataset.xhtml?persistentId=doi:10.48331/scielodata.UAAZRJ</ext-link>
        </element-citation>
</ref>
...
</ref-list>
```

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 2.3. Critérios SciELO Brasil e o modus operandi de Ciência Aberta;* [SciELO Data](https://data.scielo.org/); [FAQ SciELO Data](https://www.scielo.org/pt/sobre-o-scielo/scielo-data-pt/faq/);  [Lista de repositórios para depósito de dados de pesquisa](https://wp.scielo.org/wp-content/uploads/Lista-de-Repositorios-Recomendados_pt.pdf); [Guia de citação de dados de pesquisa](https://wp.scielo.org/wp-content/uploads/guia-de-citacao-de-dados_pt.pdf); [Guia para promoção da abertura, transparência e reprodutibilidade das pesquisas publicadas pelos periódicos SciELO](https://wp.scielo.org/wp-content/uploads/Guia_TOP_pt.pdf); [Promovendo e acelerando o compartilhamento de dados de pesquisa](https://blog.scielo.org/blog/2019/06/13/promovendo-e-acelerando-o-compartilhamento-de-dados-de-pesquisa/#.YxtW7XbMJhE); |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto); [fn-group\> \+ \<fn\>: Notas de Documento](#\<fn-group\>-+-\<fn\>:-notas-de-documento); [\<ref-list\>: Lista de Referências](#\<ref-list\>:-lista-de-referências): \<element-citation publication-type="data"\>; [\<xref\>: Referência Cruzada](#\<xref\>:-referência-cruzada); [\<graphic\> e \<inline-graphic\>: Figura](#\<graphic\>-e-\<inline-graphic\>:-figura); [\<media\> e \<inline-media\>: Objeto Multimídia](#\<media\>-e-\<inline-media\>:-objeto-multimídia); [\<supplementary-material\>: Material Suplementar](#\<supplementary-material\>:-material-suplementar); [\<ext-link\>](#\<ext-link\>:-link). |
| :---- |

### **Exemplos textuais para declaração de disponibilidade de dados de documentos publicados na coleção [SciELO Brasil](https://www.scielo.br/):** {#exemplos-textuais-para-declaração-de-disponibilidade-de-dados-de-documentos-publicados-na-coleção-scielo-brasil:}

#### **@data-available : Dados Disponíveis** {#@data-available-:-dados-disponíveis}

**Exemplo 1:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/rac/a/7MqddYp5tCWfqhGrFKrxG8M/?format=pdf&lang=pt) **(Uso de QR Code)**  
**![Exemplo da nota de Disponibilidade de Dados de um PDF de artigo publicado em SciELO da Revista de AdministraçãoContemporânea. São 3 parágrafos onde o segundo contém no lado esquerdo a imagem de um QR Code que leva ao conjunto de dados.][image12]**

**Texto:**  
**Disponibilidade dos Dados**   
Os autores afirmam que todos os dados utilizados na pesquisa foram disponibilizados publicamente, e podem ser acessados por meio da plataforma Harvard Dataverse:   
Siedschlag, Djeison; Augusto Junior, Roberto Gonçalves; Lana, Jeferson; Marcon, Rosilene, 2022, "Replication Data for: "Like, share and react: Twitter capture for research and corporate decisions" published by RAC-Revista de Administração Contemporânea", Harvard Dataverse, V1. https://doi.org/10.7910/DVN/UZUJL3   
A RAC incentiva o compartilhamento de dados mas, por observância a ditames éticos, não demanda a divulgação de qualquer meio de identificação de sujeitos de pesquisa, preservando a privacidade dos sujeitos de pesquisa. A prática de open data é viabilizar a reproducibilidade de resultados, e assegurar a irrestrita transparência dos resultados da pesquisa publicada, sem que seja demandada a identidade de sujeitos de pesquisa.

**XML:**

```
<fn fn-type="data-availability" specific-use="data-available" id="fn8"> 
<label>Disponibilidade dos Dados</label>
		<p>Os autores afirmam que todos os dados utilizados na pesquisa foram disponibilizados publicamente, e podem ser acessados por meio da plataforma Harvard Dataverse:</p>
		<p><inline-graphic xlink:href="1982-7849-rac-27-02-e220008-gf1qr.jpg"/>Siedschlag, Djeison; Augusto Junior, Roberto Gonçalves; Lana, Jeferson; Marcon, Rosilene, 2022, &quot;Replication Data for: &quot;Like, share and react: Twitter capture for research and corporate decisions&quot; published by RAC-Revista de Administração Contemporânea&quot;, Harvard Dataverse, V1. <ext-link ext-link-type="uri" xlink:href="https://doi.org/10.7910/DVN/UZUJL3">https://doi.org/10.7910/DVN/UZUJL3</ext-link></p>				
		<p>A RAC incentiva o compartilhamento de dados mas, por observância a ditames éticos, não demanda a divulgação de qualquer meio de identificação de sujeitos de pesquisa, preservando a privacidade dos sujeitos de pesquisa. A prática de open data é viabilizar a reproducibilidade de resultados, e assegurar a irrestrita transparência dos resultados da pesquisa publicada, sem que seja demandada a identidade de sujeitos de pesquisa.</p>
</fn>
```

**Exemplo 2:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/rdgv/a/djKrjyLzTNYDnRxFHMg4J8c/?format=pdf&lang=pt) 

**Texto:**  
**DECLARAÇÃO DE DISPONIBILIDADE DE DADOS:**  
O conjunto de dados deste artigo está disponível no SciELO Dataverse da Revista Direito GV, no link: [https://doi.org/10.48331/scielodata.5JYPOD](https://doi.org/10.48331/scielodata.5JYPOD).

**XML:**

```
<sec sec-type="data-availability" specific-use="data-available">
<title>Declaração de Disponibilidade de Dados:</title>
<p>O conjunto de dados deste artigo está disponível no SciELO Dataverse da <italic>Revista Direito GV</italic>, no <italic>link</italic>: <ext-link ext-link-type="uri" xlink:href="https://doi.org/10.48331/scielodata.5JYPOD">https://doi.org/10.48331/scielodata.5JYPOD</ext-link>.</p>
</sec>
```

**Exemplo 3:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/rbso/a/M3kxNFrgyxLKPmKpTFRKRBH/?format=pdf&lang=pt)

**Texto:**  
**Disponibilidade de dados**   
Todo o conjunto de dados que dá suporte aos resultados deste estudo está disponível no link [https://ccvisat.ufba.br/sinan-2/](https://ccvisat.ufba.br/sinan-2/).

**XML:**

```
<sec sec-type="data-availability" specific-use="data-available">
<title>Disponibilidade de dados</title>
<p>Todo o conjunto de dados que dá suporte aos resultados deste estudo está disponível no link <ext-link ext-link-type="uri" xlink:href="https://ccvisat.ufba.br/sinan-2/">https://ccvisat.ufba.br/sinan-2/</ext-link>.</p>
</sec>
```

#### **@data-available-upon-request : Dados Disponíveis Mediante Solicitação** {#@data-available-upon-request-:-dados-disponíveis-mediante-solicitação}

**Exemplo 1:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/riem/a/yrKzP8NngpCvNXYH4gwZsry/?format=pdf&lang=en)

**Texto:**  
**Data Availability:** The data that support the findings of this study are available from the corresponding author, Parfitt, F.M., upon reasonable request.

**XML:**

```
<fn fn-type="data-availability" specific-use="data-available-upon-request">
	<label>Data Availability:</label> 
		<p> The data that support the findings of this study are available from the corresponding author, Parfitt, F.M., upon reasonable request.</p>
</fn>
```

**Exemplo 2:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/acb/a/5z6zxYpcdhskRTxCx8yJcMS/?format=pdf&lang=en)

**Texto:**  
**Data availability statement**   
Data will be available upon request.

**XML:**

```
<sec sec-type="data-availability" specific-use="data-available-upon-request">
	<title>Data availability statement</title>
	<p>Data will be available upon request.</p>
</sec>
```

**Exemplo 3:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/bjos/a/qmrcDN33wNt5FkNMNqkxsTJ/?format=pdf&lang=en)

**Texto:**  
**Data Availability**   
Datasets related to this article will be available upon request to the corresponding author.

**XML:**

```
<fn fn-type="data-availability" specific-use="data-available-upon-request">
	<label>Data Availability</label>
		<p>Datasets related to this article will be available upon request to the corresponding author.</p>
</fn>
```

#### **@uninformed : Dados não Informados / Não Utilizou Dados** {#@uninformed-:-dados-não-informados-/-não-utilizou-dados}

**Exemplo 1:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/hh/a/VdKNvN4LpsvxX8WbRnNsdZc/?format=pdf&lang=pt)

**Texto:**  
**Disponibilidade de dados de pesquisa e outros materiais**   
Não se aplica

**XML:**

```
<fn fn-type="data-availability" specific-use="uninformed">
	<label>Disponibilidade de dados de pesquisa e outros materiais</label>
		<p>Não se aplica.</p>
</fn>
```

**Exemplo 2:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/ccedes/a/hr8zmFDnmYCHgmgwPSt4fPf/?format=pdf&lang=pt)

**Texto:**  
**Disponibilidade de Dados de Pesquisa**   
Não se aplica.

**XML:**

```
<fn fn-type="data-availability" specific-use="uninformed">
	<label>Disponibilidade de Dados de Pesquisa</label>
		<p>Não se aplica.</p>
</fn>
```

#### **@data-not-available : Dados não Disponíveis** {#@data-not-available-:-dados-não-disponíveis}

**Exemplo 1:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/rbso/a/s6mRzBF8xyS3s5zQtGhppbb/?format=pdf&lang=pt)

**Texto:**  
**Disponibilidade de dados**   
Os autores declaram que o conjunto de dados do estudo não está disponível publicamente, pois contém informações sobre serviços de saúde e processos de trabalho que permitem identificar os trabalhadores entrevistados, bem como os locais onde estavam inseridos os participantes.

**XML:**

```
<fn fn-type="data-availability" specific-use="data-not-available" id="fn3">
	<label><bold>Disponibilidade de dados</bold></label>
		<p>Os autores declaram que o conjunto de dados do estudo não está disponível publicamente, pois contém informações sobre serviços de saúde e processos de trabalho que permitem identificar os trabalhadores entrevistados, bem como os locais onde estavam inseridos os participantes.</p>
</fn>
```

**Exemplo 2:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/riem/a/YT7SfrcnBrpx5DXJzRh68MH/?format=pdf&lang=en)

**Texto:**  
**Data Availability:** Data supporting the findings of this study cannot be shared at this time due to technical or time limitations. The sharing of data necessary to reproduce these findings will be shared later

**XML:**

```
<fn fn-type="data-availability" specific-use="data-not-available">
	<label>Data Availability:</label>
		<p>Data supporting the findings of this study cannot be shared at this time due to technical or time limitations. The sharing of data necessary to reproduce these findings will be shared later.</p>
</fn>
```

**Exemplo 3:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/rap/a/X9mNTssN8S6hZPqNBSGZZJC/?format=pdf&lang=pt)

**Texto:**  
**DISPONIBILIDADE DE DADOS**   
O conjunto de dados que dá suporte aos resultados deste estudo não está disponível publicamente.

**XML:**

```
<fn fn-type="data-availability" specific-use="data-not-available" id="fn1">
	<label>DISPONIBILIDADE DE DADOS</label>
		<p>O conjunto de dados que dá suporte aos resultados deste estudo não está disponível publicamente.</p>
</fn>
```

#### **@data-in-article : Dados no Artigo** {#@data-in-article-:-dados-no-artigo}

**Exemplo 1:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/cebape/a/gCQPySxskQs5pHmqWJMKHrm/?format=pdf&lang=pt)

**Texto:**  
**DISPONIBILIDADE DE DADOS**   
Todo o conjunto de dados que dá suporte aos resultados deste estudo foi publicado no próprio artigo. 

**XML:**

```
<fn fn-type="data-availability" specific-use="data-in-article" id="fn4" >
	<label>DISPONIBILIDADE DE DADOS</label>
		<p>Todo o conjunto de dados que dá suporte aos resultados deste estudo foi publicado no próprio artigo.</p>
</fn>
```

**Exemplo 2:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/epec/a/jCzhjgcV3bLNRSzFCtgTbZK/?format=pdf&lang=pt)

**Texto:**  
**DECLARAÇÃO SOBRE DISPONIBILIDADE DE DADOS**   
O corpus que dá suporte aos resultados deste estudo foi publicado no próprio artigo.

**XML:**

```
<fn fn-type="data-availability" specific-use="data-in-article" id="fn7">
	<label>Declaração sobre disponibilidade de dados</label>
		<p>O corpus que dá suporte aos resultados deste estudo foi publicado no próprio artigo.</p>
</fn>
```

**Exemplo 3:**

[**Exemplo da diagramação no PDF**](https://www.scielo.br/j/adr/a/CcmwtnLdP7wrYFJdMv9HCbm/?format=pdf&lang=en)

**Texto:**  
**Data Availability**   
All data generated or analysed during this study are included in this published article.

**XML:**

```
<sec sec-type="data-availability" specific-use="data-in-article">
	<title>Availability of data and materials</title>
		<p>All data generated or analysed during this study are included in this published article.</p>
</sec>
```

## **Declaração de Editor Responsável pelo Processo de Avaliação**  {#declaração-de-editor-responsável-pelo-processo-de-avaliação}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Os(As) editores(as) responsáveis pelo processo de avaliação, que acompanham e gerenciam os documentos aprovados, devem ter seus nomes informados nos documentos publicados. Essa prática informa sobre a transparência do processo de avaliação dos manuscritos.

Quando o(a) editor(a) responsável pelo processo de avaliação do documento aprovado não aceitar ter seu nome publicado, a editora ou editor-chefe pode disponibilizar o seu próprio nome, assim como pode ocorrer o nome da editora ou editor-chefe publicado em conjunto com a do editor responsável pelo processo de avaliação.

A declaração dos(as) editores(as) responsáveis pelo processo de avaliação, nos documentos publicados em SciELO Brasil, deve ser uma prática para todos os periódicos indexados na coleção, sendo **obrigatório** o uso da declaração para os [tipos de documentos](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>) com valores de @article-type iguais a:

1. data-article  
2. brief-report   
3. case-report  
4. rapid-communication  
5. research-article  
6. review-article

Os demais [tipos de documentos](#equivalência-entre-documentos-indexáveis-e-@article-type-em-\<article\>) indexáveis, opcionalmente podem conter a declaração de editor responsável pelo processo de avaliação.

A declaração deve ser marcada em uma [\<fn\>](#\<author-notes\>-+-\<fn\>:-notas-de-autor) obrigatoriamente com o atributo e valor @fn-type="edited-by" em [\<author-notes\>](#\<author-notes\>-+-\<fn\>:-notas-de-autor). 

**Exemplo**: [PDF](https://www.scielo.br/j/rbepid/a/gJtwFMx6CgxtFQm6jVBwLQP/?format=pdf&lang=en) 

![Exemplo de um artigo em PDF publicado em SciELO da Revista Brasileira de Epidemiologia com a Declaração de Editor Responsável pelo Processo de Avaliação. São duas linhas, ambas mostram o título do cargo das editoras em azul marinho com texto em caixa alta separado por dois pontos, seguido do nome completo da editora mais o logo clicável do ORCID das editoras.][image13]

**Exemplo:** XML

```
<author-notes>
<fn fn-type="edited-by">
<label>ASSOCIATE EDITOR:</label>
<p>Luana Patricia Marmitt <ext-link ext-link-type="uri" xlink:href="http://orcid.org/0000-0003-0526-7954">http://orcid.org/0000-0003-0526-7954</ext-link></p>
</fn>
<fn fn-type="edited-by">
<label>SCIENTIFIC EDITOR:</label>
<p>Juraci Almeida Cesar <ext-link ext-link-type="uri" xlink:href="http://orcid.org/0000-0003-0864-0486">http://orcid.org/0000-0003-0864-0486</ext-link></p>
</fn>
</author-notes>
```

| Atenção:  Não use \<title\>, \<p\>, \<bold\> ou \<italic\> para identificar ou representar rótulos das notas [\<fn\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) de autor, documento ou tabela; use \<label\>; O grupo de nota representado pelo elemento [\<author-notes\>](#\<fn\>:-nota-de-autor,-documento-e-tabela) deve ocorrer uma única vez no documento. |
| :---- |

| Consulte:  [Critérios SciELO Brasi](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 2\. Os Critérios SciELO Brasil no contexto do Programa SciELO* e *5.2.4. Relevância, sustentabilidade e qualificação editorial.* |
| :---- |

| Consulte no SPS:  [\<author-notes\> \+ \<fn\>: Notas de Autor](#\<author-notes\>-+-\<fn\>:-notas-de-autor).  |
| :---- |

## **Ensaio Clínico** {#ensaio-clínico}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

O Ensaio Clínico é um estudo em voluntários com o objetivo de responder a questões específicas de saúde, os periódicos devem exigir que o manuscrito informe o número de identificação do registro como condição para proceder com a avaliação e o registro deve ser identificado pelo elemento [\<ext-link\>](#\<ext-link\>:-link). 

Para identificação de um Ensaio Clínico, o elemento [\<ext-link\>](#\<ext-link\>:-link) deve apresentar:

Atributos obrigatórios:

1. @ext-link-type="clinical-trial"  
2. @xlink:href com a URL do registro de Ensaio Clínico

**Exemplo:**

```
<p>Número de registro clínico:<ext-link ext-link-type="clinical-trial" xlink:href="https://clinicaltrials.gov/ct2/show/NCT00981734">NCT00981734</ext-link></p>
```

| Consulte:  [Critérios SciELO Brasi](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.6.4.2. Registro de ensaios clínicos.* |
| :---- |

| Consulte no SPS:  [\<ext-link\>](#\<ext-link\>:-link). |
| :---- |

## **Errata** {#errata}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Errata para documento publicado.

| Consulte:  [Guia para Publicação de Errata](https://wp.scielo.org/wp-content/uploads/guia_errata.pdf); [Critérios SciELO Brasi](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.6.4.8. Erratas*; [SciELO Ética](https://www.scielo.org/pt/sobre-o-scielo/scielo-etica/). |
| :---- |

### **XML da Errata** {#xml-da-errata}

O XML da errata deve possuir a tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. Em [\<article\>](#\<article\>:-artigo) @article-type="correction"  
2. [\<article-id pub-id-type="doi"\>](#\<article-id\>:-doi-e-other) com o DOI da errata  
3. [\<subject\>](#\<article-categories\>:-seção-de-documento) com a mesma seção do PDF da errata  
4. [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) com o mesmo título do PDF da errata  
5. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   1. @related-article-type="corrected-article"   
   2. @id  
   3. @xlink:href com número DOI do documento mencionado pela errata  
   4. ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="correction" xml:lang="pt">
...
    <front>
        <article-meta>
            <article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Errata</subject>
                </subj-group>
                ...
            </article-categories>
            <title-group>
                <article-title>Errata: Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
            </title-group>
            ...
            <permissions>
                ...
            </permissions>
            <related-article related-article-type="corrected-article" id="r1" xlink:href="10.1590/abd1806-4841.20142998" ext-link-type="doi"/>
            <counts>           
        </article-meta>
        ...
    </front>
    <body>
<p>Texto da Errata ...</p>
    </body>
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<article-categories\>: Seção de Documento](#\<article-categories\>:-seção-de-documento); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento). |
| :---- |

### **XML do Documento Mencionado pela Errata** {#xml-do-documento-mencionado-pela-errata}

O XML do(s) documento(s) mencionado(s) pela errata deve(m) ter a adição da tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="correction-forward"   
   * @id  
   * @xlink:href com número DOI da errata  
   * ext-link-type="doi" 

Deve-se adicionar ainda em \<front\> dentro de [\<history\>](#\<history\>:-datas-de-histórico) a data de aprovação da errata do documento com:

1.  @date-type="corrected"

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="pt">
	...
	<front>
		<article-meta>
			<article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>Artigo Original</subject>
				</subj-group>
				...
			</article-categories>
			<title-group>
				<article-title>Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
			</title-group>			
			   ...
               <history>
				<date date-type="received">
			        ...
				<date date-type="accepted">
			        ...
				<date date-type="corrected">
					<day>12</day>
					<month>12</month>
					<year>2025</year>
				</date>
			</history>
			</permissions>			
              ...
            <related-article related-article-type="correction-forward" id="r1" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
             <counts>							
          </article-meta>
		...
	 </front>
	 <body>
		...
	 </body>
	 <back>
		...
	</back>
</article>
```

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html). |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

## **Manifestação de Preocupação** {#manifestação-de-preocupação}

Manifestação de Preocupação para documento publicado.

| Consulte:  [Guia para Publicação de Manifestação de Preocupação](https://wp.scielo.org/wp-content/uploads/guia_manifestacao.pdf); [SciELO Ética](https://www.scielo.org/pt/sobre-o-scielo/scielo-etica/). |
| :---- |

| Atenção:  A especificação JATS não define um valor específico de @article-type ou @related-article-type para a Nota de Exoneração. Como esse tipo de documento é raro e sem padronização internacional, não há orientação de marcação no SciELO Publishing Schema; Caso uma Nota de Exoneração precise ser publicada, recomenda-se que o periódico entre previamente em contato com a equipe SciELO, preferencialmente por meio do [Comitê de Ética SciELO](https://www.scielo.org/pt/sobre-o-scielo/scielo-etica/sobre-o-scielo-etica/), para definição do procedimento adequado. |
| :---- |

### **XML da Manifestação de Preocupação** {#xml-da-manifestação-de-preocupação}

O XML da manifestação de preocupação deve possuir a tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. Em [\<article\>](#\<article\>:-artigo) @article-type="expression-of-concern"  
2. [\<article-id pub-id-type="doi"\>](#\<article-id\>:-doi-e-other) com o DOI da manifestação  
3. [\<subject\>](#\<article-categories\>:-seção-de-documento) com a mesma seção do PDF da manifestação  
4. [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) com o mesmo título do PDF da manifestação  
5. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   1. @related-article-type="object-of-concern"   
   2. @id  
   3. @xlink:href com número DOI do documento mencionado pela manifestação  
   4. ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="expression-of-concern" xml:lang="pt">
...
    <front>
        <article-meta>
            <article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Manifestação de Preocupação</subject>
                </subj-group>
                ...
            </article-categories>
            <title-group>
                <article-title>Manifestação de Preocupação: Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
            </title-group>
            ...
            <permissions>
                ...
            </permissions>
            <related-article related-article-type="object-of-concern" id="r1" xlink:href="10.1590/abd1806-4841.20142998" ext-link-type="doi"/>
            <counts>           
        </article-meta>
        ...
    </front>
    <body>
<p>Texto da Manifestação de Preocupação ...</p>
    </body>
</article>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<article-categories\>: Seção de Documento](#\<article-categories\>:-seção-de-documento); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento). |
| :---- |

### **XML do Documento Mencionado pela Manifestação de Preocupação** {#xml-do-documento-mencionado-pela-manifestação-de-preocupação}

O XML do documento mencionado pela manifestação de preocupação deve ter a adição da tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="expression-of-concern"   
   * @id  
   * @xlink:href com número DOI da manifestação  
   * ext-link-type="doi" 

Deve-se adicionar ainda em \<front\> dentro de [\<history\>](#\<history\>:-datas-de-histórico) a data de aprovação da manifestação de preocupação do documento com:

1.  @date-type="expression-of-concern"

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="pt">
	...
	<front>
		<article-meta>
			<article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>Artigo Original</subject>
				</subj-group>
				...
			</article-categories>
			<title-group>
				<article-title>Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
			</title-group>			
			   ...
               <history>
				<date date-type="received">
			        ...
				<date date-type="accepted">
			        ...
				<date date-type="expression-of-concern">
					<day>12</day>
					<month>12</month>
					<year>2025</year>
				</date>
			</history>
			</permissions>			
              ...
            <related-article related-article-type="expression-of-concern" id="r1" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
             <counts>							
          </article-meta>
		...
	 </front>
	 <body>
		...
	 </body>
	 <back>
		...
	</back>
</article>
```

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html). |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

## **Parecer: Revisão por Pares Aberta** {#parecer:-revisão-por-pares-aberta}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

O termo Revisão por Pares Aberta (Open Peer Review), pode indicar distintos tipos e níveis de abertura, por exemplo:

1. Pode significar que as identidades dos autores e pareceristas são reveladas a ambos;  
2. Os pareceres são disponibilizados em seguida aos documentos publicados;  
3. Utilização de plataformas de interação aberta com público;  
4. Publicação do nome do editor responsável pelo processo de revisão.

Existem 3 formatos de marcação para publicação de revisão por pares:

1. **Parecer como [\<article\>](#\<article\>:-artigo) (Recomendado)**  
   1. 1 PDF para o documento;  
   2. 1 ou mais PDFs para os pareceres (um PDF para cada parecer/parecerista).  
2. **Parecer como [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo)**  
   1. 1 único PDF para documento e parecer(es).  
3. **Parecer como link externo [\<related-article\>](#\<related-article\>:-relação-entre-documentos)**  
   1. 1 único PDF para documento e link do(s) parecer(es).

Para a publicação de pareceres no SciELO, o periódico deve considerar que as revisões por pares contenham no mínimo os seguintes aspectos:

* DOI:  
  * Se parecer como [\<article\>](#\<article\>:-artigo) é obrigatório atribuição de DOI para cada parecer/parecerista e um DOI distinto para a tradução dos pareceres \- se houver tradução;  
  * Se parecer como [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) não deve-se atribuir um DOI.  
* Os pareceristas podem ou não ter seus nomes divulgados (anônimo);  
* O parecerista deve ter seu papel divulgado mesmo que anônimo (revisor ou editor);  
* A data em que o parecer foi recomendado deve ser informada;  
* O status da recomendação do parecer deve ser informado (Aceito, aceito com recomendações, etc.).

Os pareceres marcados como [\<article\>](#\<article\>:-artigo) ou [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem obrigatoriamente possuir os elementos:

* @article-type="reviewer-report";  
* @contrib-type="author";  
* [\<role specific-use="reviewer"\>](#\<role\>:-papel-do-autor---taxonomia-credit) ou [\<role specific-use="editor"\>](#\<role\>:-papel-do-autor---taxonomia-credit);  
* [\<history\>](#\<history\>:-datas-de-histórico) \+ \<date date-type="reviewer-report-received"\>;   
* \<custom-meta-group\> \+ \<custom-meta\> \+ \<meta-name\> e \<meta-value\>  
  * Os termos possíveis para \<meta-value\> são:  
    * revision  
    * major-revision  
    * minor-revision  
    * reject  
    * reject-with-resubmit  
    * accept  
    * formal-accept  
    * accept-in-principle

Para parecer como [\<article\>](#\<article\>:-artigo) além dos elementos mencionados anteriormente, adiciona-se as tags de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) com a licença [CC-BY](https://creativecommons.org/licenses/by/4.0/deed.en) e [\<related-article\>](#\<related-article\>:-relação-entre-documentos) referenciando o documento revisado, com os seguintes atributos obrigatórios:

* @related-article-type="reviewed-article";  
* @id;  
* @xlink:href com número DOI do documento revisado;  
* @ext-link-type="doi".

Já para a indicação do parecer, que é somente um link externo dentro do documento publicado, utiliza-se apenas a marcação do [\<related-article\>](#\<related-article\>:-relação-entre-documentos), no próprio documento revisado. Neste caso, considerar:

* @related-article-type="reviewer-report";  
* @id;  
* @xlink:href com número DOI ou link da URL do parecer (se houver um DOI dar preferência para marcar o DOI);  
* @ext-link-type="doi" ou @ext-link-type="uri".

| Atenção:  Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplos:**

**Parecer como [\<article\>](#\<article\>:-artigo)**

***Parecer A** do parecerista **Marcos Silva** para o documento: Proin diam magna, congue sit amet mi aliquam, dapibus rhoncus risus.*

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML"
    dtdversion="1.3" specific-use="sps-1.10" article-type="reviewer-report" xml:lang="pt">
    <front>
        <article-meta>
            <article-id pub-id-type="doi">10.1590/123456720182998OPR1</article-id>
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Parecer</subject>
                </subj-group>
            </article-categories>
            <title-group>
                <article-title>Parecer A: Proin diam magna, congue sit amet mi aliquam, dapibus rhoncus risus</article-title>
            </title-group>
            <contrib-group>
                <contrib contrib-type="author">
 	  <contrib-id contrib-id-type="orcid">0000-0001-0002-0003</contrib-id>               
      <name>
                        <surname>Silva</surname>
                        <given-names>Marcos</given-names>
                    </name>
                    <role specific-use="reviewer">Parecerista</role>
                    <xref ref-type="aff" rid="aff1"/>
                </contrib>
            </contrib-group>
            <aff id="aff1"> ... </aff>
          	<history>
                <date date-type="reviewer-report-received">
                    <day>10</day>
                    <month>01</month>
                    <year>2025</year>
                </date>
            </history>  
         	<permissions>
			<license license-type="open-access" xlink:href="https://creativecommons.org/licenses/by/4.0/" xml:lang="en">
			<license-p>This is an open-access article distributed under the terms of the Creative Commons Attribution License</license-p>
			</license>
</permissions>
            <related-article related-article-type="reviewed-article" id="r1"
                xlink:href="10.1590/abd1806-4841.20142998" ext-link-type="doi"/>            
            <custom-meta-group>
                <custom-meta>
                    <meta-name>peer-review-recommendation</meta-name>
                    <meta-value>reject-with-resubmit</meta-value>
                </custom-meta>
            </custom-meta-group>
        </article-meta>
    </front>
    <body>
        <sec>
            <title>Parecer A</title>
<p>Vivamus elementum sapien tellus, a suscipit elit auctor in. Cras est nisl, egestas non ultrices ut, fringilla eu magna. Morbi llamcorper et diam a elementum. Phasellus vitae diam eget arcu dignissim ultrices.</p>
<p>Sed in laoreet sem. Morbi vel imperdiet magna. Curabitur a velit maximus, volutpat metus in, posuere sem. Etiam eget lacus lorem. Nulla facilisi..</p>
<p>Recomendação: Submeter novamente para avaliação</p>
        </sec>
    </body> 
</article>
```

**Parecer como [\<article\>](#\<article\>:-artigo)**

***Parecer B** do parecerista **Marcos Silva** para o documento: Proin diam magna, congue sit amet mi aliquam, dapibus rhoncus risus.*

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML"
    dtdversion="1.3" specific-use="sps-1.10" article-type="reviewer-report" xml:lang="pt">
    <front>
        <article-meta>
            <article-id pub-id-type="doi">10.1590/123456720182998OPR2</article-id>
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Parecer</subject>
                </subj-group>
            </article-categories>
            <title-group>
                <article-title>Parecer B: Proin diam magna, congue sit amet mi aliquam, dapibus rhoncus risus. Donec viverra</article-title>
            </title-group>
            <contrib-group>
                <contrib contrib-type="author">
	 <contrib-id contrib-id-type="orcid">0000-0001-0002-0003</contrib-id>
                    <name>
                        <surname>Silva</surname>
                        <given-names>Marcos</given-names>
                    </name>
                    <role specific-use="reviewer">Parecerista</role>
                    <xref ref-type="aff" rid="aff1"/>
                </contrib>
            </contrib-group>
            <aff id="aff1"> ... </aff>
          	<history>
                <date date-type="reviewer-report-received">
                    <day>23</day>
                    <month>01</month>
                    <year>2025</year>
                </date>
            </history>  
         	 <permissions>
			<license license-type="open-access" xlink:href="https://creativecommons.org/licenses/by/4.0/" xml:lang="en">
			<license-p>This is an open-access article distributed under the terms of the Creative Commons Attribution License</license-p>
			</license>
 </permissions>
            <related-article related-article-type="reviewed-article" id="r1"
                xlink:href="10.1590/abd1806-4841.20142998" ext-link-type="doi"/>            
            <custom-meta-group>
                <custom-meta>
                    <meta-name>peer-review-recommendation</meta-name>
                    <meta-value>accept</meta-value>
                </custom-meta>
            </custom-meta-group>
        </article-meta>
    </front>
    <body>
        <sec>
            <title>Parecer B</title>
<p>Vivamus elementum sapien tellus, a suscipit elit auctor in. Cras est nisl, egestas non ultrices ut, fringilla eu magna. Morbi llamcorper et diam a elementum. Phasellus vitae diam eget arcu dignissim ultrices.</p>
<p>Sed in laoreet sem. Morbi vel imperdiet magna. Curabitur a velit maximus, volutpat metus in, posuere sem. Etiam eget lacus lorem. Nulla facilisi..</p>
<p>Recomendação: Aceito</p>
        </sec>
    </body> 
</article>
```

**Parecer como [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo)**

***Parecer 1, 2 e 3** dos pareceristas **Marcos Silva, Mirian Costa e anônimo** para o documento: Proin diam magna, congue sit amet mi aliquam, dapibus rhoncus risus.*

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML"
    dtdversion="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="en">
    <front>
        <article-meta>
            <article-id pub-id-type="doi">10.1590/123456720182998O54</article-id>
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Original Article</subject>
                </subj-group>
            </article-categories>
            <title-group>
                <article-title>Proin diam magna, congue sit amet mi aliquam, dapibus rhoncus risus. Donec viverra</article-title>
            </title-group>
           ...
<sub-article article-type="reviewer-report" id="S1" xml:lang="en">  
    <front-stub>  
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Peer-Review</subject>
                </subj-group>
                ...
            </article-categories>
            <title-group>
                <article-title>Review I</article-title>
            </title-group>
            <contrib-group>
                <contrib contrib-type="author">
                    <contrib-id contrib-id-type="orcid">0000-0001-0002-0003</contrib-id>
                    <name>
                        <surname>Silva</surname>
                        <given-names>Marcos</given-names>
                    </name>
                    <role specific-use="reviewer">Reviewer</role>
                    <xref ref-type="aff" rid="aff1"/>
                </contrib>
            </contrib-group>
            <aff id="aff1">...</aff>            
            <history>
                <date date-type="reviewer-report-received">
                    <day>10</day>
                    <month>01</month>
                    <year>2025</year>
                </date>
            </history>
         	 <permissions>
			<license license-type="open-access" xlink:href="https://creativecommons.org/licenses/by/4.0/" xml:lang="en">
			<license-p>This is an open-access article distributed under the terms of the Creative Commons Attribution License</license-p>
			</license>
 </permissions>
            <custom-meta-group>
                <custom-meta>
                    <meta-name>peer-review-recommendation</meta-name>
                    <meta-value>accept</meta-value>
                </custom-meta>
            </custom-meta-group> ...        
    </front-stub>
    <body>
        <sec>
            <title>Review I</title>
            <p>Vivamus elementum sapien tellus, a suscipit elit auctor in. Cras est nisl, egestas non ultrices ut, fringilla eu magna. Morbi llamcorper et diam a elementum. Phasellus vitae diam eget arcu dignissim ultrices.</p>
<p>Sed in laoreet sem. Morbi vel imperdiet magna. Curabitur a velit maximus, volutpat metus in, posuere sem. Etiam eget lacus lorem. Nulla facilisi..</p>
<p>Recomendação: Aceito</p>
        </sec>
    </body>
</sub-article>
<sub-article article-type="reviewer-report" id="S2" xml:lang="en">  
    <front-stub> 
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Peer-Review</subject>
                </subj-group>
                ...
            </article-categories>
            <title-group>
                <article-title>Review II</article-title>
            </title-group>
            <contrib-group>
                <contrib contrib-type="author">
                    <contrib-id contrib-id-type="orcid">0000-0000-0005-0027</contrib-id>
                    <name>
                        <surname>Costa</surname>
                        <given-names>Miriam</given-names>
                    </name>
                    <role specific-use="reviewer">Reviewer</role>
                    <xref ref-type="aff" rid="aff1"/>
                </contrib>
            </contrib-group>
            <aff id="aff1">...</aff>            
            <history>
                <date date-type="reviewer-report-received">
                    <day>13</day>
                    <month>01</month>
                    <year>2025</year>
                </date>
            </history>
         	<permissions> ... </permissions>
            <custom-meta-group>
                <custom-meta>
                    <meta-name>peer-review-recommendation</meta-name>
                    <meta-value>accept</meta-value>
                </custom-meta>
            </custom-meta-group> ...        
    </front-stub>
    <body>
        <sec>
            <title>Review II</title>
            <p>Vivamus elementum sapien tellus, a suscipit elit auctor in. Cras est nisl, egestas non ultrices ut, fringilla eu magna. Morbi llamcorper et diam a elementum. Phasellus vitae diam eget arcu dignissim ultrices.</p>
<p>Sed in laoreet sem. Morbi vel imperdiet magna. Curabitur a velit maximus, volutpat metus in, posuere sem. Etiam eget lacus lorem. Nulla facilisi..</p>
<p>Recomendação: Aceito</p>
        </sec>
    </body>
</sub-article>
<sub-article article-type="reviewer-report" id="S3" xml:lang="en">  
    <front-stub> 
            <article-categories>
                <subj-group subj-group-type="heading">
                    <subject>Peer-Review</subject>
                </subj-group>
                ...
            </article-categories>
            <title-group>
                <article-title>Review III</article-title>
            </title-group>
            <contrib-group>
                <contrib contrib-type="author">                   
                    <anonymous/>
                    <role specific-use="reviewer">Reviewer</role>
                    <xref ref-type="aff" rid="aff1"/>
                </contrib>
            </contrib-group>
            <aff id="aff1">...</aff>            
            <history>
                <date date-type="reviewer-report-received">
                    <day>25</day>
                    <month>01</month>
                    <year>2025</year>
                </date>
            </history>
         	<permissions> ... </permissions>
            <custom-meta-group>
                <custom-meta>
                    <meta-name>peer-review-recommendation</meta-name>
                    <meta-value>accept</meta-value>
                </custom-meta>
            </custom-meta-group> ...        
    </front-stub>
    <body>
        <sec>
            <title>Review III</title>
            <p>Vivamus elementum sapien tellus, a suscipit elit auctor in. Cras est nisl, egestas non ultrices ut, fringilla eu magna. Morbi llamcorper et diam a elementum. Phasellus vitae diam eget arcu dignissim ultrices.</p>
<p>Sed in laoreet sem. Morbi vel imperdiet magna. Curabitur a velit maximus, volutpat metus in, posuere sem. Etiam eget lacus lorem. Nulla facilisi..</p>
<p>Recomendação: Aceito</p>
        </sec>
    </body>
</sub-article>
</article>
```

**Parecer com link externo [\<related-article\>](#\<related-article\>:-relação-entre-documentos) dentro do documento**

**Seção [\<sec\>](#\<sec\>:-seção-de-texto)**: **com @ext-link-type="doi"** 

```
<sec>
   <title>Relatório de Revisão por Pares</title>
<p>Parecerista:</p>
		<p>Gabriel Flores, Fundação Escola de Sociologia e Política de São Paulo (FESPSP), São Paulo / SP - Brasil), <ext-link ext-link-type="uri" xlink:href="https://orcid.org/0000-0003-4787-8446">ORCID: 0000-0003-4787-8446</ext-link>
   <p>O relatório de revisão por pares está disponível em: <related-article related-article-type="reviewer-report" id="r1"ext-link-type="doi"            xlink:href="10.1590/SciELO123456.1174">10.1590/SciELO123456.1174</related-article></p>
</sec>
```

**Nota [\<fn\>](#\<fn-group\>-+-\<fn\>:-notas-de-documento): com @ext-link-type="uri"**

```
<fn-group>
 	<fn fn-type="other">
 		<label>Relatório de Revisão por Pares</label>
			<p>Parecerista:</p>
			<p>Gabriel Flores, Fundação Escola de Sociologia e Política de São Paulo (FESPSP), São Paulo / SP - Brasil), <ext-link ext-link-type="uri" xlink:href="https://orcid.org/0000-0003-4787-8446">ORCID: 0000-0003-4787-8446</ext-link>
 			<p>O relatório de revisão por pares está disponível em: <related-article related-article-type="reviewer-report" id="r1" ext-link-type="uri"
xlink:href="https://publons.com/publon/000000/#review-2020xxx">Publons</related-article></p>
 	</fn>
</fn-group>
```

| Consulte:  [Critérios SciELO Brasi](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 2\. Os Critérios SciELO Brasil no contexto do Programa SciELO*,  *2.3. Critérios SciELO Brasil e o modus operandi de Ciência Aberta*,  *5.2.6. Avaliação de manuscritos informada*; e *5.3.1.1. Alinhamento com o modus operandi de ciência aberta*; [Guia para promoção da abertura, transparência e reprodutibilidade das pesquisas publicadas pelos periódicos SciELO.](https://wp.scielo.org/wp-content/uploads/Guia_TOP_pt.pdf) |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [fn-group\> \+ \<fn\>: Notas de Documento](#\<fn-group\>-+-\<fn\>:-notas-de-documento); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<role\>: Papel do Autor \- Taxonomia CRediT](#\<role\>:-papel-do-autor---taxonomia-credit); [\<sec\>: Seção de Texto](#\<sec\>:-seção-de-texto); [\<sub-article\> \+ \<front-stub\>: Sub Artigo](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

## **Preprint: Documentos Publicados Anteriormente como Preprint** {#preprint:-documentos-publicados-anteriormente-como-preprint}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Documentos que foram publicados anteriormente como Preprints, quando enviados para a publicação em SciELO, devem considerar obrigatoriamente dois aspectos:

1. Indicação do link do Preprint **(preferencialmente DOI)**;  
2. Indicação da data de publicação do Preprint que irá compor as datas de histórico do documento.

As tags correspondentes para estes itens no XML são:

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com atributo @related-article-type="preprint"   
2. [\<history\>](#\<history\>:-datas-de-histórico) com a tag [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html) com atributo @date-type="preprint" 

**Exemplos:**

[**\<related-article\>**](#\<related-article\>:-relação-entre-documentos) **com DOI do preprint**

```
<related-article related-article-type="preprint" id="r1" xlink:href="10.1590/SciELOPreprints.1174" ext-link-type="doi"/>
```

[**\<related-article\>**](#\<related-article\>:-relação-entre-documentos) **com URL do preprint**

```
<related-article related-article-type="preprint" id="r1" xlink:href="https://preprints.scielo.org/index.php/scielo/preprint/view/1174/version/1253" ext-link-type="uri"/>
```

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo); Não use o atributo @ext-link-type="uri" para inserir link DOI exemplo: @xlink:href="[https://doi.org/10.1590/SciELOPreprints.1174](https://doi.org/10.1590/SciELOPreprints.1174)"; Se existir um DOI use o atributo @ext-link-type="doi" com @xlink:href="10.1590/SciELOPreprints.1174". |
| :---- |

**Data de histórico no documento com a data de publicação do preprint**

```
<history>
	<date date-type="received">
 		<day>04</day>
 		<month>08</month>
 		<year>2024</year>
 	</date>
 	<date date-type="accepted">
 		<day>18</day>
 		<month>02</month>
 		<year>2025</year>
 	</date>
 	<date date-type="preprint">
 		<day>22</day>
 		<month>06</month>
 		<year>2024</year>
 	</date>
</history>
```

| Consulte:  [Critérios SciELO Brasil](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 2\. Os Critérios SciELO Brasil no contexto do Programa SciELO*, *2.1. Princípios do Programa SciELO*, *2.3. Critérios SciELO Brasil e o modus operandi de Ciência Aberta*, *5.2.2. Caráter científico – artigos de pesquisa e alinhamento com a Ciência Aberta* e *5.3.1.1. Alinhamento com o modus operandi de ciência aberta*. |
| :---- |

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html). |
| :---- |

| Consulte no SPS:  [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos). |
| :---- |

## **Retratação** {#retratação}

[![Bandeira do Brasil: Seções com marcação XML que tem especificidades relacionadas aos Critérios, política e procedimentos para a admissão e a permanência de periódicos na Coleção SciELO Brasil.][image2]](https://www.scielo.br/about/criterios-scielo-brasil)Esta seção possui especificidades relacionadas aos Critérios SciELO Brasil.

Retratação total ou parcial para um documento publicado.

| Consulte:  [Guia para Publicação de Retratação](https://wp.scielo.org/wp-content/uploads/guia_retratacao.pdf); [Critérios SciELO Brasi](https://www.scielo.br/media/files/20240900-Criterios-SciELO-Brasil.pdf)*: 5.2.6.4.9. Retratações*; [SciELO Ética](https://www.scielo.org/pt/sobre-o-scielo/scielo-etica/). |
| :---- |

### **XML da Retratação Total e Parcial**  {#xml-da-retratação-total-e-parcial}

O XML da retratação, seja ela total ou parcial, deve possuir a tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. Se Retratação Total: Em [\<article\>](#\<article\>:-artigo) @article-type="retraction"  
2. Se Retratação Parcial: Em [\<article\>](#\<article\>:-artigo) @article-type="partial-retraction"

Para ambos os tipos, os dados a seguir são iguais:

1. [\<article-id pub-id-type="doi"\>](#\<article-id\>:-doi-e-other) com o DOI da retratação  
2. [\<subject\>](#\<article-categories\>:-seção-de-documento) com a mesma seção do PDF da retratação  
3. [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) com o mesmo título do PDF da retratação  
4. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="retracted-article"   
   * @id  
   * @xlink:href com número DOI do documento mencionado pela retratação  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="retraction" xml:lang="pt">
	...
	<front>
		<article-meta>
			<article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>Retratação</subject>
				</subj-group>
				...
			</article-categories>
			<title-group>
				<article-title>Retratação: Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
			</title-group>
			...
			<permissions>
				...
			</permissions>
			<related-article related-article-type="retracted-article" id="r01" xlink:href="10.1590/abd1806-4841.20142998" ext-link-type="doi"/>
			<counts>
				...
		</article-meta>
		...
	</front>
```

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<article\>: Artigo](#\<article\>:-artigo); [\<article-id\>: DOI e Other](#\<article-id\>:-doi-e-other); [\<article-categories\>: Seção de Documento](#\<article-categories\>:-seção-de-documento); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento). |
| :---- |

### **XML do Documento Retratado Totalmente** {#xml-do-documento-retratado-totalmente}

O XML da retratação total deve manter apenas as informações de \<front\> e ter a exclusão de todo o conteúdo do documento de:

1. \<body\>  
2. \<back\>

Em \<body\> adiciona-se apenas o texto da retratação.

Em \<front\> adiciona-se em [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) e [\<trans-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) (se houver), o texto da retratação no idioma do título do documento. Sugestão:

* Para título em português: **ARTIGO RETRATADO**  
* Para título em inglês: **RETRACTED ARTICLE**  
* Para título em espanhol: **ARTÍCULO RETRACTADO**

Deve-se adicionar ainda em \<front\> dentro de [\<history\>](#\<history\>:-datas-de-histórico) a data de aprovação da retratação do documento com: 

1.  @date-type="retracted"

Além das exclusões e alterações acima, deve-se adicionar a tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="retraction-forward"   
   * @id  
   * @xlink:href com número DOI da retratação  
   * ext-link-type="doi" 

| Atenção:  [\<related-article\>](#\<related-article\>:-relação-entre-documentos) deve ser inserido abaixo de [\<permissions\>](#\<permissions\>:-licença-creative-commons-e-copyright) ou acima de \<counts\>; Respeitar a ordem dos atributos em [\<related-article\>](#\<related-article\>:-relação-entre-documentos): 1°: @related-article-type 2°: @id 3°: @xlink:href 4°: @ext-link-type Documentos com tradução [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo) devem ter a adição do [\<related-article\>](#\<related-article\>:-relação-entre-documentos) também no idioma traduzido. Neste caso, não repita em [\<related-article\>](#\<related-article\>:-relação-entre-documentos) o mesmo @id de [\<article\>](#\<article\>:-artigo) em [\<sub-article\>](#\<sub-article\>-+-\<front-stub\>:-sub-artigo). |
| :---- |

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="pt">
	...
	<front>
		<article-meta>
			<article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>Artigo Original</subject>
				</subj-group>
				...
			</article-categories>
			<title-group>
				<article-title>ARTIGO RETRATADO: Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
			</title-group>			
			...
                                   <history>
				<date date-type="received">
			                       ...
				<date date-type="accepted">
			                       ...
				<date date-type="retracted">
					<day>01</day>
					<month>12</month>
					<year>2024</year>
				</date>
			</history>
			</permissions>
			<related-article related-article-type="retraction-forward" id="r1" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
			<counts>
			...				
		</article-meta>
		...
	</front>
	<body>
		...
	</body>
</article>
```

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html). |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento). |
| :---- |

### **XML do Documento Retratado Parcialmente** {#xml-do-documento-retratado-parcialmente}

O XML da retratação parcial deve manter todo o conteúdo do documento, exceto onde houver a retratação. O trecho textual (parágrafo), figura, tabela, etc., deve ser excluído do XML.

Em \<front\> adiciona-se em [\<article-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) e [\<trans-title\>](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento) (se houver), o texto da retratação no idioma do título do documento. Sugestão:

*  Para título em português: **ARTIGO PARCIALMENTE RETRATADO**  
*  Para título em inglês: **PARTIALLY RETRACTED ARTICLE**  
*  Para título em espanhol: **ARTÍCULO PARCIALMENTE RETRACTADO**

Deve-se adicionar ainda em \<front\> dentro de [\<history\>](#\<history\>:-datas-de-histórico) a data de aprovação da retratação do documento com: 

1.  @date-type="retracted"

Além das exclusões e alterações acima, deve-se adicionar a tag de [\<related-article\>](#\<related-article\>:-relação-entre-documentos) sem nenhum texto adicional e deve conter:

1. [\<related-article\>](#\<related-article\>:-relação-entre-documentos) com:  
   * @related-article-type="partial-retraction"   
   * @id  
   * @xlink:href com número DOI da retratação  
   * ext-link-type="doi" 

**Exemplo:**

```
<article xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:mml="http://www.w3.org/1998/Math/MathML" dtd-version="1.3" specific-use="sps-1.10" article-type="research-article" xml:lang="pt">`
	...
	<front>
		<article-meta>
			<article-id pub-id-type="doi">10.1590/123456720182998e</article-id>
			<article-categories>
				<subj-group subj-group-type="heading">
					<subject>Artigo Original</subject>
				</subj-group>
				...
			</article-categories>
			<title-group>
				<article-title>ARTIGO PARCIALMENTE RETRATADO: Proin maximus, urna vehicula blandit dapibus, felis nisi venenatis risus, quis vestibulum libero mi fermentum augue</article-title>
			</title-group>                                         	
			...
			<history>
				<date date-type="received">
					...
				<date date-type="accepted">
						...
				<date date-type="retracted">
					<day>01</day>
					<month>12</month>
					<year>2024</year>
				</date>
			</history>
			</permissions>
			<related-article related-article-type="partial-retraction" id="r1" xlink:href="10.1590/123456720182998e" ext-link-type="doi"/>
			<counts>
				...                                                               	
		</article-meta>
		...
	</front>
	<body>
		...
	</body>
	<back>
		...
	</back>
</article>
```

| Consulte na JATS:  [\<date\>](https://jats.nlm.nih.gov/publishing/tag-library/1.3/element/date.html). |
| :---- |

| Consulte no SPS:  [Nomeação de Arquivos](#nomeação-de-arquivos); [\<history\>: Datas de Histórico](#\<history\>:-datas-de-histórico); [\<related-article\>: Relação entre Documentos](#\<related-article\>:-relação-entre-documentos); [\<title-group\> e \<trans-title-group\>: Título de Documento](#\<title-group\>-e-\<trans-title-group\>:-título-de-documento). |
| :---- |