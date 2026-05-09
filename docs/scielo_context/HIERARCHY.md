# HIERARCHY — pais e filhos por tag

> Fonte: `src/scielo/bin/markup/app_core/tree.txt`. Esse arquivo controla a
> UI dinâmica do `markup.prg` (cada nó-pai vira uma toolbar; cada filho
> vira um botão), e — mais importante para você — é a **regra
> hierárquica usada pelo `VerifyHierarchy`** em
> `_analysis/markup_macros.txt:1492`. Quando o operador clica numa tag, o
> template **rejeita a inserção** se o ancestral imediato detectado no
> documento (via `verify_hier_previous_element`) não estiver listado aqui
> como pai permitido.

> ⚠️ `tree.txt` ≠ DTD oficial: alguns filhos listados aqui não estão na
> DTD 4.0 (ex.: `prefix`/`suffix`/`credit` dentro de `author` — em
> `common4_0.dtd` o `<!ELEMENT author … (fname? & surname & ctrbid*)>`
> não os inclui). Para gerar XML que passe pelo `parser.exe`/DTD, use
> sempre `_analysis/DTD_SCHEMA.md` como fonte autoritativa de
> o-que-é-filho-válido. Para gerar marcação que sobreviva à edição
> manual no Word com o `markup.prg`, use este arquivo.

> Convenções:
> - `*<nome>` (ex.: `*authors`, `*kwdgrp`) — pseudo-botão que dispara
>   macro automática; ver `TAG_INDEX.md`. Não é tag SGML real.
> - `*(attr)*` — o template oferece formulário de atributos.
> - `*(children)*` — pode descer um nível.

## Índice reverso (qual pai pode conter esta tag)

```
TAG                    PAIS PERMITIDOS
*authors               ref
*boxedtxt              ifloat
*deflist               deflist, glossary, ifloat, xmlbody
*fn                    back, doc, docresp, fngrp, subdoc
*fngrp                 back, doc, docresp, subdoc
*kwdgrp                doc, docresp, subdoc
*list                  ifloat
*oauthor               ocontrib, omonog
*page-fn               ifloat
*pauthor               authors
*publoc-pubname        ref
*pubname-publoc        ref
*source                ref
abnt6023               back
abstract               bbibcom, bibcom, subdoc
accepted               hist
acitat                 abnt6023
ack                    back, doc, docresp, subdoc
acontrib               acitat
aff                    front, text
afftrans               subdoc
aiserial               acitat
alttext                figgrp, graphic
alttitle               doctitle
amonog                 acitat
anonym                 oauthor
anonymous              author
apa                    back
app                    appgrp, docresp
appgrp                 back, doc, subdoc
article                start
arttitle               ref
attrib                 figgrp, versegrp
authgrp                front, text
author                 acontrib, amonog, authgrp, doc, docresp, icontrib, iiserial, imonog, pcontrib, pmonog, subdoc, vcontrib, vmonog
authorid               author
authors                product, ref
award                  funding
back                   article, response, subart, text
bbibcom                back
bibcom                 front
body                   article
boxedtxt               ifloat
caption                figgrp, figgrps, supplmat, tabwrap
cauthor                authors
chptitle               product, ref
cited                  aiserial, amonog, iiserial, imonog, oiserial, omonog, piserial, pmonog, ref, viserial, vmonog
city                   aff, aiserial, amonog, confgrp, iiserial, imonog, normaff, oiserial, omonog, pmonog, thesgrp, thesis, vmonog
cltrial                ifloat
code                   ifloat
coltitle               amonog, imonog, omonog, pmonog, vmonog
colvolid               amonog, pmonog
confgrp                acitat, amonog, bbibcom, bibcom, ocitat, omonog, pmonog, ref, vmonog
confname               confgrp
contract               award, ref, report, rsponsor
corpauth               acontrib, amonog, authgrp, doc, docresp, icontrib, iiserial, imonog, pcontrib, pmonog, subdoc, vcontrib, vmonog
corresp                ifloat
country                aff, aiserial, amonog, confgrp, iiserial, imonog, normaff, oiserial, omonog, pmonog, thesgrp, thesis, vmonog
cpholder               cpright
cpright                ifloat
cpyear                 cpright
credit                 author
ctreg                  cltrial
custom                 customgrp
customgrp              ifloat
date                   aiserial, amonog, confgrp, icontrib, iiserial, imonog, ocontrib, oiserial, omonog, patgrp, pcontrib, pmonog, product, ref, thesgrp, thesis, viserial, vmonog
def                    defitem
defitem                deflist
deflist                deflist, glossary, ifloat, xmlbody
degree                 thesgrp, thesis
deposit                article
doc                    start
docresp                doc, subdoc
doctitle               doc, docresp, subdoc
doi                    aiserial, amonog, doc, docresp, front, iiserial, imonog, oiserial, omonog, piserial, pmonog, subdoc, text, viserial, vmonog
edition                amonog, iiserial, imonog, omonog, pmonog, ref, vmonog
elemattr               element
element                ifloat
elocatid               ref
email                  aff, corresp, normaff
equation               ifloat
et-al                  acontrib, amonog, authors, icontrib, iiserial, imonog, ocontrib, omonog, vcontrib, vmonog
extent                 aiserial, amonog, imonog, oiserial, omonog, product, ref, viserial, vmonog
figgrp                 figgrps, ifloat
figgrps                ifloat
fn                     back, doc, docresp, fngrp, subdoc
fname                  author, awarded, oauthor, pauthor, sig, subresp
fname-spanish-surname  author, awarded, oauthor, pauthor, sig, subresp
fname-surname          author, awarded, oauthor, pauthor, sig, subresp
fngrp                  back, doc, docresp, subdoc
fntable                tabwrap
front                  article, response, subart
funding                fn, p
fundsrc                award
glossary               back, doc, docresp, glossary, subdoc
graphic                equation, figgrp, ifloat, tabwrap
hist                   bbibcom, bibcom, doc, docresp, subdoc
histdate               hist
icitat                 iso690
icontrib               icitat
ign                    ifloat
iiserial               icitat
imonog                 icitat
inpress                viserial, vmonog
institid               ifloat
isbn                   amonog, imonog, omonog, product, ref
isdesig                iiserial
iso690                 back
issn                   aiserial, iiserial, oiserial, ref
isstitle               aiserial, iiserial, oiserial
issueno                aiserial, iiserial, oiserial, piserial, ref, viserial
keygrp                 bbibcom, bibcom
keyword                keygrp
kwd                    kwdgrp
kwdgrp                 doc, docresp, subdoc
label                  aff, afftrans, app, corresp, equation, figgrp, figgrps, fn, fntable, glossary, li, normaff, ref, supplmat, tabwrap, versegrp
li                     list
license                licenses
licensep               license
licenses               back
licinfo                ifloat
list                   ifloat
media                  ifloat
medium                 iiserial, imonog
mmlmath                equation
moreinfo               product, ref
name                   custom
no                     acitat, confgrp, icitat, ocitat, pcitat, report, vcitat
normaff                doc, subdoc
notes                  aiserial, amonog, iiserial, imonog, pmonog
oauthor                ocontrib, omonog
ocitat                 other
ocontrib               ocitat
ocorpaut               ocontrib, omonog
oiserial               ocitat
omonog                 ocitat
onbehalf               authgrp, author, doc, docresp, subdoc
oprrole                author
orgdiv                 awarded, corpauth, ocorpaut, rsponsor, sponsor, thesgrp, thesis
orgdiv1                normaff
orgdiv2                normaff
orgname                awarded, corpauth, normaff, ocorpaut, patgrp, rsponsor, sponsor, thesgrp, thesis
other                  back
othinfo                oiserial, omonog
p                      ack, app, boxedtxt, caption, sec, subsec, xmlabstr, xmlbody
pages                  acontrib, aiserial, amonog, iiserial, imonog, ocontrib, oiserial, omonog, piserial, pmonog, ref, viserial, vmonog
part                   amonog, imonog, omonog, pmonog, ref, viserial, vmonog
patent                 patgrp
patentno               ref
patgrp                 acontrib, amonog, imonog, ocontrib, omonog, vcontrib, vmonog
pauthor                authors
pcitat                 apa
pcontrib               pcitat
piserial               pcitat
pmonog                 pcitat
prefix                 author, pauthor, sig
previous               author, corpauth, oauthor, ocorpaut
product                ifloat
projname               report
pubid                  aiserial, amonog, iiserial, imonog, oiserial, omonog, piserial, pmonog, ref, viserial, vmonog
publoc                 product, ref
pubname                aiserial, amonog, iiserial, imonog, oiserial, omonog, pmonog, product, ref, vmonog
quote                  ifloat
received               hist
ref                    refs
refs                   ifloat
related                doc, docresp, front, ifloat, subdoc
report                 amonog, bbibcom, bibcom, imonog, omonog, pmonog, vmonog
reportid               ref
response               article, subart
revised                hist
role                   aff, author, normaff, sigblock
rsponsor               report
sec                    app, boxedtxt, xmlabstr, xmlbody
sectitle               ack, app, deflist, fngrp, glossary, kwdgrp, refs, sec, subsec, xmlabstr
series                 product, ref
sertitle               aiserial, iiserial, oiserial, piserial
sig                    sigblock
sigblock               xmlbody
source                 product, ref
sponsor                confgrp
state                  aff, aiserial, amonog, confgrp, iiserial, imonog, normaff, omonog, pmonog, thesgrp, thesis, vmonog
stitle                 aiserial, iiserial, oiserial, vstitle
subart                 article, subart
subdoc                 doc, subdoc
subresp                amonog, icontrib, imonog
subsec                 sec
subtitle               acontrib, amonog, doctitle, icontrib, imonog, ocontrib, omonog, pcontrib, pmonog, titlegrp, vtitle
suffix                 author, pauthor, sig
suppl                  aiserial, oiserial, piserial, ref, viserial
supplmat               ifloat
surname                author, awarded, oauthor, pauthor, sig, subresp
surname-fname          author, awarded, oauthor, pauthor, sig, subresp
table                  tabwrap
tabwrap                ifloat
td                     tr
term                   defitem
texmath                equation
text                   start
text-ref               ref
th                     tr
thesgrp                bbibcom, bibcom, ref
thesis                 amonog, omonog, pmonog, vmonog
title                  acontrib, amonog, icontrib, imonog, ocontrib, omonog, pcontrib, pmonog, titlegrp, vtitle
titlegrp               front, text
toctitle               doc, docresp, front, subdoc
tome                   amonog
tp                     vstitle, vtitle
tr                     table
update                 iiserial, imonog
uri                    ifloat
url                    aiserial, amonog, iiserial, imonog, oiserial, omonog, piserial, pmonog, ref, viserial, vmonog
value                  custom
vancouv                back
vcitat                 vancouv
vcontrib               vcitat
versegrp               ifloat, versegrp
version                ref, vmonog
versline               versegrp
viserial               vcitat
vmonog                 vcitat
volid                  acontrib, aiserial, amonog, iiserial, imonog, oiserial, omonog, piserial, pmonog, ref, viserial, vmonog
vstitle                viserial
vtitle                 vcontrib, vmonog
xhtml                  equation, tabwrap
xmlabstr               bibcom, doc, docresp, subdoc
xmlbody                article, doc, docresp, response, subart, subdoc
xref                   ifloat
zipcode                aff, normaff
```

## Mapa pai → filhos

### `start`

- `doc`
- `article`
- `text`

### `ifloat`

- `*boxedtxt` *children*
- `boxedtxt` *children*
- `cltrial`
- `code` *children*
- `corresp`
- `cpright`
- `customgrp`
- `deflist` *children*
- `*deflist` *children*
- `*page-fn` *children*
- `element` *children*
- `equation` *children*
- `figgrp` *children*
- `figgrps` *children*
- `graphic` *children*
- `ign` *children*
- `institid` *children*
- `licinfo` *children*
- `*list` *children*
- `list` *children*
- `product` *children*
- `quote` *children*
- `refs` *children*
- `related` *children*
- `supplmat` *children*
- `media` *children*
- `tabwrap` *children*
- `uri` *children*
- `versegrp` *children*
- `xref` *children*

### `customgrp`

- `custom` *children*

### `custom`

- `name`
- `value`

### `name`

_(sem filhos — guarda PCDATA ou só inline)_

### `value`

_(sem filhos — guarda PCDATA ou só inline)_

### `*fngrp`

_(sem filhos — guarda PCDATA ou só inline)_

### `*fn`

_(sem filhos — guarda PCDATA ou só inline)_

### `abnt6023`

- `acitat` *children*

### `abstract`

_(sem filhos — guarda PCDATA ou só inline)_

### `accepted`

_(sem filhos — guarda PCDATA ou só inline)_

### `acitat`

- `no`
- `acontrib`
- `amonog`
- `aiserial`
- `confgrp`

### `ack`

- `sectitle`
- `p`

### `acontrib`

- `author` *children*
- `corpauth` *children*
- `et-al`
- `title`
- `subtitle`
- `volid`
- `pages`
- `patgrp`

### `aff`

- `label`
- `role`
- `city`
- `state`
- `country`
- `zipcode`
- `email`

### `aiserial`

- `sertitle`
- `stitle`
- `isstitle`
- `date` *children*
- `volid` *children*
- `issueno` *children*
- `suppl` *children*
- `pages` *children*
- `extent` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*
- `issn` *children*
- `city` *children*
- `state` *children*
- `country` *children*
- `pubname` *children*
- `notes` *children*

### `amonog`

- `author` *children*
- `corpauth` *children*
- `et-al`
- `title`
- `subtitle`
- `date` *children*
- `edition` *children*
- `volid` *children*
- `part` *children*
- `tome` *children*
- `coltitle` *children*
- `colvolid` *children*
- `pages` *children*
- `extent` *children*
- `city` *children*
- `state` *children*
- `country` *children*
- `pubname` *children*
- `patgrp`
- `confgrp` *children*
- `thesis` *children*
- `report` *children*
- `isbn` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*
- `subresp` *children*
- `notes` *children*

### `anonym`

_(sem filhos — guarda PCDATA ou só inline)_

### `apa`

- `pcitat` *children*

### `article`

- `deposit`
- `front`
- `xmlbody`
- `body`
- `back`
- `response` *children*
- `subart` *children*

### `elemattr`

_(sem filhos — guarda PCDATA ou só inline)_

### `app`

- `label`
- `sectitle`
- `p` *children*
- `sec` *children*

### `authgrp`

- `author` *children*
- `onbehalf`
- `corpauth` *children*

### `author`

- `anonymous`
- `fname-surname`
- `fname-spanish-surname`
- `surname-fname`
- `fname`
- `surname`
- `prefix`
- `suffix`
- `credit` *attr* *children*
- `role`
- `oprrole`
- `previous`
- `authorid` *attr* *children*
- `onbehalf`

### `credit`

_(sem filhos — guarda PCDATA ou só inline)_

### `oprrole`

_(sem filhos — guarda PCDATA ou só inline)_

### `authorid`

_(sem filhos — guarda PCDATA ou só inline)_

### `awarded`

- `orgname`
- `orgdiv`
- `fname-surname`
- `fname-spanish-surname`
- `surname-fname`
- `fname`
- `surname`

### `back`

- `ack`
- `vancouv`
- `iso690`
- `abnt6023`
- `apa`
- `other`
- `*fngrp` *children*
- `fngrp` *children*
- `*fn` *children*
- `fn` *children*
- `licenses`
- `bbibcom`
- `glossary`
- `appgrp` *children*

### `bbibcom`

- `abstract` *children*
- `keygrp` *children*
- `report` *children*
- `confgrp`
- `thesgrp`
- `hist`

### `bibcom`

- `abstract` *children*
- `xmlabstr` *children*
- `keygrp` *children*
- `report` *children*
- `confgrp`
- `thesgrp`
- `hist`

### `body`

_(sem filhos — guarda PCDATA ou só inline)_

### `caption`

- `p`

### `cited`

_(sem filhos — guarda PCDATA ou só inline)_

### `city`

_(sem filhos — guarda PCDATA ou só inline)_

### `cltrial`

- `ctreg` *children*

### `coltitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `colvolid`

_(sem filhos — guarda PCDATA ou só inline)_

### `confgrp`

- `confname` *children*
- `no` *children*
- `date` *children*
- `city` *children*
- `state` *children*
- `country` *children*
- `sponsor` *children*

### `confname`

_(sem filhos — guarda PCDATA ou só inline)_

### `contract`

_(sem filhos — guarda PCDATA ou só inline)_

### `corpauth`

- `orgname`
- `orgdiv`
- `previous`

### `corresp`

- `label`
- `email` *children*

### `country`

_(sem filhos — guarda PCDATA ou só inline)_

### `ctrbid`

_(sem filhos — guarda PCDATA ou só inline)_

### `ctreg`

_(sem filhos — guarda PCDATA ou só inline)_

### `date`

_(sem filhos — guarda PCDATA ou só inline)_

### `degree`

_(sem filhos — guarda PCDATA ou só inline)_

### `deposit`

_(sem filhos — guarda PCDATA ou só inline)_

### `doi`

_(sem filhos — guarda PCDATA ou só inline)_

### `dperiod`

_(sem filhos — guarda PCDATA ou só inline)_

### `edition`

_(sem filhos — guarda PCDATA ou só inline)_

### `element`

- `elemattr` *children*

### `email`

_(sem filhos — guarda PCDATA ou só inline)_

### `equation`

- `graphic`
- `xhtml` *children*
- `texmath`
- `mmlmath`
- `label`

### `et-al`

_(sem filhos — guarda PCDATA ou só inline)_

### `extent`

_(sem filhos — guarda PCDATA ou só inline)_

### `figgrp`

- `graphic`
- `alttext`
- `attrib`
- `label`
- `caption`

### `figgrps`

- `label`
- `caption`
- `figgrp` *children*

### `fname`

_(sem filhos — guarda PCDATA ou só inline)_

### `fngrp`

- `sectitle`
- `*fn` *children*
- `fn` *children*

### `fn`

- `label`
- `funding`

### `fntable`

- `label`

### `front`

- `related` *children*
- `toctitle`
- `doi` *children*
- `titlegrp`
- `authgrp`
- `aff` *children*
- `bibcom`

### `glossary`

- `label`
- `sectitle`
- `glossary` *children*
- `deflist`
- `*deflist`

### `deflist`

- `sectitle`
- `defitem` *children*
- `deflist` *children*
- `*deflist` *children*

### `*deflist`

_(sem filhos — guarda PCDATA ou só inline)_

### `defitem`

- `term`
- `def`

### `term`

_(sem filhos — guarda PCDATA ou só inline)_

### `def`

_(sem filhos — guarda PCDATA ou só inline)_

### `graphic`

- `alttext`

### `hist`

- `received`
- `revised` *children*
- `accepted`
- `histdate`

### `icitat`

- `no`
- `icontrib`
- `imonog`
- `iiserial`

### `icontrib`

- `author` *children*
- `corpauth` *children*
- `et-al`
- `subresp` *children*
- `date` *children*
- `title`
- `subtitle`

### `ign`

_(sem filhos — guarda PCDATA ou só inline)_

### `iiserial`

- `isstitle`
- `author` *children*
- `corpauth` *children*
- `et-al`
- `medium`
- `sertitle`
- `stitle`
- `city` *children*
- `state` *children*
- `country` *children*
- `edition` *children*
- `pubname` *children*
- `date` *children*
- `update` *children*
- `volid` *children*
- `issueno` *children*
- `pages` *children*
- `isdesig` *children*
- `notes` *children*
- `issn` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*

### `imonog`

- `author` *children*
- `corpauth` *children*
- `et-al`
- `subresp` *children*
- `title`
- `subtitle`
- `medium`
- `edition` *children*
- `city` *children*
- `state` *children*
- `country` *children*
- `pubname` *children*
- `date` *children*
- `update` *children*
- `volid` *children*
- `part` *children*
- `pages` *children*
- `extent` *children*
- `coltitle` *children*
- `report` *children*
- `notes` *children*
- `url`
- `cited` *children*
- `doi` *children*
- `pubid` *children*
- `isbn` *children*
- `patgrp`

### `inpress`

_(sem filhos — guarda PCDATA ou só inline)_

### `isbn`

_(sem filhos — guarda PCDATA ou só inline)_

### `isdesig`

_(sem filhos — guarda PCDATA ou só inline)_

### `iso690`

- `icitat` *children*

### `issn`

_(sem filhos — guarda PCDATA ou só inline)_

### `isstitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `issueno`

_(sem filhos — guarda PCDATA ou só inline)_

### `keygrp`

- `keyword` *children*

### `keyword`

_(sem filhos — guarda PCDATA ou só inline)_

### `label`

_(sem filhos — guarda PCDATA ou só inline)_

### `li`

- `label`

### `license`

- `licensep`

### `licensep`

_(sem filhos — guarda PCDATA ou só inline)_

### `licenses`

- `license` *children*

### `*list`

_(sem filhos — guarda PCDATA ou só inline)_

### `list`

- `li` *children*

### `medium`

_(sem filhos — guarda PCDATA ou só inline)_

### `mmlmath`

_(sem filhos — guarda PCDATA ou só inline)_

### `no`

_(sem filhos — guarda PCDATA ou só inline)_

### `notes`

_(sem filhos — guarda PCDATA ou só inline)_

### `oauthor`

- `fname-surname`
- `fname-spanish-surname`
- `surname-fname`
- `fname`
- `surname`
- `anonym`
- `previous`

### `ocitat`

- `no`
- `ocontrib` *children*
- `omonog`
- `oiserial`
- `confgrp`

### `ocontrib`

- `*oauthor` *children*
- `oauthor` *children*
- `ocorpaut` *children*
- `et-al`
- `title`
- `subtitle`
- `date`
- `pages`
- `patgrp`

### `ocorpaut`

- `orgname`
- `orgdiv`
- `previous`

### `oiserial`

- `sertitle`
- `stitle`
- `isstitle`
- `date` *children*
- `volid` *children*
- `issueno` *children*
- `suppl` *children*
- `pages` *children*
- `extent` *children*
- `issn` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*
- `othinfo`
- `city` *children*
- `country` *children*
- `pubname` *children*

### `omonog`

- `*oauthor` *children*
- `oauthor` *children*
- `ocorpaut` *children*
- `et-al`
- `title`
- `subtitle`
- `date`
- `pages`
- `extent` *children*
- `edition` *children*
- `thesis`
- `confgrp`
- `report` *children*
- `patgrp`
- `city` *children*
- `state` *children*
- `country` *children*
- `pubname` *children*
- `coltitle` *children*
- `volid` *children*
- `part` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*
- `isbn` *children*
- `othinfo` *children*

### `onbehalf`

_(sem filhos — guarda PCDATA ou só inline)_

### `orgdiv`

_(sem filhos — guarda PCDATA ou só inline)_

### `orgname`

_(sem filhos — guarda PCDATA ou só inline)_

### `other`

- `ocitat` *children*

### `othinfo`

_(sem filhos — guarda PCDATA ou só inline)_

### `p`

- `funding` *children*

### `pages`

_(sem filhos — guarda PCDATA ou só inline)_

### `part`

_(sem filhos — guarda PCDATA ou só inline)_

### `patent`

_(sem filhos — guarda PCDATA ou só inline)_

### `patgrp`

- `orgname`
- `patent`
- `date`

### `pcitat`

- `no`
- `pcontrib`
- `pmonog`
- `piserial`

### `pcontrib`

- `author` *children*
- `corpauth` *children*
- `date`
- `title`
- `subtitle`

### `piserial`

- `sertitle`
- `volid` *children*
- `issueno` *children*
- `suppl` *children*
- `pages` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*

### `pmonog`

- `author` *children*
- `corpauth` *children*
- `date`
- `title`
- `volid` *children*
- `part` *children*
- `subtitle`
- `confgrp`
- `thesis`
- `coltitle` *children*
- `colvolid` *children*
- `pages` *children*
- `edition` *children*
- `city` *children*
- `state` *children*
- `country` *children*
- `pubname` *children*
- `report` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*
- `notes` *children*

### `previous`

_(sem filhos — guarda PCDATA ou só inline)_

### `product`

- `authors` *attr* *children*
- `chptitle` *children*
- `source` *children*
- `pubname` *children*
- `publoc` *children*
- `date`
- `isbn` *children*
- `extent` *children*
- `series`
- `moreinfo` *children*

### `projname`

_(sem filhos — guarda PCDATA ou só inline)_

### `pubid`

_(sem filhos — guarda PCDATA ou só inline)_

### `pubname`

_(sem filhos — guarda PCDATA ou só inline)_

### `quote`

_(sem filhos — guarda PCDATA ou só inline)_

### `received`

_(sem filhos — guarda PCDATA ou só inline)_

### `related`

_(sem filhos — guarda PCDATA ou só inline)_

### `report`

- `no`
- `contract` *children*
- `rsponsor` *children*
- `projname` *children*

### `response`

- `front`
- `xmlbody`
- `back`

### `revised`

_(sem filhos — guarda PCDATA ou só inline)_

### `role`

_(sem filhos — guarda PCDATA ou só inline)_

### `rsponsor`

- `orgname`
- `orgdiv`
- `contract` *children*

### `sciname`

_(sem filhos — guarda PCDATA ou só inline)_

### `sec`

- `sectitle`
- `subsec` *children*
- `p` *children*

### `sectitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `sertitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `sig`

- `fname-surname`
- `fname-spanish-surname`
- `surname-fname`
- `fname`
- `surname`
- `prefix`
- `suffix`

### `sigblock`

- `sig` *children*
- `role`

### `sponsor`

- `orgname`
- `orgdiv`

### `state`

_(sem filhos — guarda PCDATA ou só inline)_

### `stitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `subart`

- `front`
- `xmlbody`
- `back`
- `response` *children*
- `subart` *children*

### `subkey`

_(sem filhos — guarda PCDATA ou só inline)_

### `subresp`

- `fname-surname`
- `fname-spanish-surname`
- `surname-fname`
- `fname`
- `surname`

### `subsec`

- `sectitle` *children*
- `p` *children*

### `subtitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `suppl`

_(sem filhos — guarda PCDATA ou só inline)_

### `supplmat`

- `label`
- `caption`

### `media`

_(sem filhos — guarda PCDATA ou só inline)_

### `surname`

_(sem filhos — guarda PCDATA ou só inline)_

### `table`

- `tr` *children*

### `tabwrap`

- `label`
- `caption`
- `xhtml` *children*
- `graphic`
- `table`
- `fntable` *children*

### `td`

_(sem filhos — guarda PCDATA ou só inline)_

### `texmath`

_(sem filhos — guarda PCDATA ou só inline)_

### `text`

- `doi` *children*
- `titlegrp`
- `authgrp`
- `aff` *attr* *children*
- `back`

### `th`

_(sem filhos — guarda PCDATA ou só inline)_

### `thesgrp`

- `city`
- `state`
- `country`
- `date`
- `degree`
- `orgname`
- `orgdiv`

### `thesis`

- `city`
- `state`
- `country`
- `date`
- `degree`
- `orgname`
- `orgdiv`

### `title`

_(sem filhos — guarda PCDATA ou só inline)_

### `titlegrp`

- `title` *children*
- `subtitle` *children*

### `toctitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `tome`

_(sem filhos — guarda PCDATA ou só inline)_

### `tp`

_(sem filhos — guarda PCDATA ou só inline)_

### `tr`

- `th` *children*
- `td` *children*

### `update`

_(sem filhos — guarda PCDATA ou só inline)_

### `uri`

_(sem filhos — guarda PCDATA ou só inline)_

### `url`

_(sem filhos — guarda PCDATA ou só inline)_

### `vancouv`

- `vcitat` *children*

### `vcitat`

- `no`
- `vcontrib`
- `viserial`
- `vmonog`

### `vcontrib`

- `author` *children*
- `corpauth` *children*
- `et-al`
- `vtitle`
- `patgrp`

### `version`

_(sem filhos — guarda PCDATA ou só inline)_

### `viserial`

- `vstitle`
- `date` *children*
- `inpress` *children*
- `volid` *children*
- `issueno` *children*
- `suppl` *children*
- `part` *children*
- `extent` *children*
- `pages` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*

### `vmonog`

- `author` *children*
- `corpauth` *children*
- `et-al`
- `vtitle`
- `edition`
- `volid` *children*
- `part` *children*
- `version` *children*
- `confgrp` *children*
- `city` *children*
- `state` *children*
- `country` *children*
- `pubname` *children*
- `inpress` *children*
- `date` *children*
- `pages` *children*
- `extent` *children*
- `report` *children*
- `thesis` *children*
- `url` *children*
- `cited` *children*
- `doi` *children*
- `pubid` *children*
- `patgrp`
- `coltitle` *children*

### `volid`

_(sem filhos — guarda PCDATA ou só inline)_

### `vstitle`

- `stitle`
- `tp`

### `vtitle`

- `title`
- `subtitle`
- `tp`

### `xmlabstr`

- `sectitle`
- `sec` *children*
- `p` *children*

### `xmlbody`

- `sec` *children*
- `p` *children*
- `deflist` *children*
- `*deflist` *children*
- `sigblock`

### `xref`

_(sem filhos — guarda PCDATA ou só inline)_

### `zipcode`

_(sem filhos — guarda PCDATA ou só inline)_

### `surname-fname`

_(sem filhos — guarda PCDATA ou só inline)_

### `*oauthor`

_(sem filhos — guarda PCDATA ou só inline)_

### `doc`

- `author` *children*
- `ack` *attr*
- `appgrp`
- `corpauth` *children*
- `docresp` *attr* *children*
- `doctitle` *children*
- `doi`
- `*fngrp` *children*
- `fngrp` *children*
- `*fn` *children*
- `fn` *children*
- `glossary` *attr*
- `hist` *attr*
- `*kwdgrp` *children*
- `kwdgrp` *children*
- `normaff` *attr* *children*
- `onbehalf`
- `related` *children*
- `subdoc` *children*
- `toctitle`
- `xmlabstr` *children*
- `xmlbody` *attr*

### `*kwdgrp`

_(sem filhos — guarda PCDATA ou só inline)_

### `kwdgrp`

- `sectitle`
- `kwd` *children*

### `doctitle`

- `subtitle`
- `alttitle`

### `funding`

- `award` *children*

### `award`

- `contract`
- `fundsrc` *children*

### `docresp`

- `ack`
- `app` *children*
- `author` *children*
- `corpauth` *children*
- `doctitle` *children*
- `doi`
- `*fngrp` *children*
- `fngrp` *children*
- `*fn` *children*
- `fn` *children*
- `glossary`
- `hist`
- `*kwdgrp` *children*
- `kwdgrp` *children*
- `onbehalf`
- `related` *children*
- `toctitle`
- `xmlabstr` *children*
- `xmlbody`

### `subdoc`

- `abstract` *children*
- `ack`
- `appgrp` *children*
- `author` *children*
- `afftrans` *attr* *children*
- `corpauth` *children*
- `docresp` *children*
- `doctitle` *children*
- `doi`
- `*fngrp` *children*
- `fngrp` *children*
- `*fn` *children*
- `fn` *children*
- `glossary`
- `hist`
- `*kwdgrp` *children*
- `kwdgrp` *children*
- `normaff` *attr* *children*
- `onbehalf`
- `related` *children*
- `subdoc` *children*
- `toctitle`
- `xmlabstr` *children*
- `xmlbody`

### `kwd`

_(sem filhos — guarda PCDATA ou só inline)_

### `fundsrc`

_(sem filhos — guarda PCDATA ou só inline)_

### `refs`

- `sectitle`
- `ref` *children*

### `ref`

- `text-ref`
- `label`
- `*authors` *attr*
- `authors` *attr*
- `arttitle` *attr*
- `chptitle` *attr*
- `cited` *children*
- `series` *children*
- `confgrp` *attr*
- `contract`
- `date` *attr* *children*
- `edition` *children*
- `elocatid` *children*
- `extent` *children*
- `issn` *attr* *children*
- `isbn` *attr* *children*
- `issueno` *children*
- `moreinfo` *attr* *children*
- `pages` *children*
- `part` *children*
- `patentno`
- `pubid` *children*
- `publoc` *children*
- `pubname` *children*
- `*publoc-pubname` *children*
- `*pubname-publoc` *children*
- `reportid`
- `*source`
- `source`
- `suppl` *children*
- `thesgrp`
- `url` *attr* *children*
- `version` *children*
- `volid` *attr* *children*

### `elocatid`

_(sem filhos — guarda PCDATA ou só inline)_

### `text-ref`

_(sem filhos — guarda PCDATA ou só inline)_

### `*authors`

_(sem filhos — guarda PCDATA ou só inline)_

### `authors`

- `*pauthor` *children*
- `pauthor` *children*
- `cauthor` *children*
- `et-al` *children*

### `pauthor`

- `fname-surname`
- `fname-spanish-surname`
- `surname-fname`
- `fname`
- `surname`
- `prefix`
- `suffix`

### `*pauthor`

_(sem filhos — guarda PCDATA ou só inline)_

### `cauthor`

_(sem filhos — guarda PCDATA ou só inline)_

### `doctit`

_(sem filhos — guarda PCDATA ou só inline)_

### `source`

_(sem filhos — guarda PCDATA ou só inline)_

### `*source`

_(sem filhos — guarda PCDATA ou só inline)_

### `reportid`

_(sem filhos — guarda PCDATA ou só inline)_

### `letterto`

_(sem filhos — guarda PCDATA ou só inline)_

### `found-at`

_(sem filhos — guarda PCDATA ou só inline)_

### `patentno`

_(sem filhos — guarda PCDATA ou só inline)_

### `moreinfo`

_(sem filhos — guarda PCDATA ou só inline)_

### `afftrans`

- `label`

### `normaff`

- `label`
- `role`
- `orgname`
- `orgdiv1`
- `orgdiv2`
- `city`
- `state`
- `country`
- `zipcode`
- `email`

### `orgdiv1`

_(sem filhos — guarda PCDATA ou só inline)_

### `orgdiv2`

_(sem filhos — guarda PCDATA ou só inline)_

### `arttitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `chptitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `publoc`

_(sem filhos — guarda PCDATA ou só inline)_

### `*publoc-pubname`

_(sem filhos — guarda PCDATA ou só inline)_

### `*pubname-publoc`

_(sem filhos — guarda PCDATA ou só inline)_

### `boxedtxt`

- `sec` *children*
- `p` *children*

### `*boxedtxt`

_(sem filhos — guarda PCDATA ou só inline)_

### `appgrp`

- `app` *children*

### `series`

_(sem filhos — guarda PCDATA ou só inline)_

### `versegrp`

- `label`
- `versline` *children*
- `versegrp` *children*
- `attrib` *children*

### `versline`

_(sem filhos — guarda PCDATA ou só inline)_

### `attrib`

_(sem filhos — guarda PCDATA ou só inline)_

### `alttitle`

_(sem filhos — guarda PCDATA ou só inline)_

### `alttext`

_(sem filhos — guarda PCDATA ou só inline)_

### `fname-surname`

_(sem filhos — guarda PCDATA ou só inline)_

### `fname-spanish-surname`

_(sem filhos — guarda PCDATA ou só inline)_

### `cpright`

- `cpyear`
- `cpholder`

### `cpyear`

_(sem filhos — guarda PCDATA ou só inline)_

### `cpholder`

_(sem filhos — guarda PCDATA ou só inline)_

### `licinfo`

_(sem filhos — guarda PCDATA ou só inline)_

### `suffix`

_(sem filhos — guarda PCDATA ou só inline)_

### `prefix`

_(sem filhos — guarda PCDATA ou só inline)_

### `*page-fn`

_(sem filhos — guarda PCDATA ou só inline)_

### `xhtml`

_(sem filhos — guarda PCDATA ou só inline)_

### `institid`

_(sem filhos — guarda PCDATA ou só inline)_

### `code`

_(sem filhos — guarda PCDATA ou só inline)_

### `histdate`

_(sem filhos — guarda PCDATA ou só inline)_

### `anonymous`

_(sem filhos — guarda PCDATA ou só inline)_

