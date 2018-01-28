
; TODO implement OR nodes
;   $SONORANT/u$SONORANT/$V_(#|$C)
; TODO implement NOT nodes
;   $SONORANT/u$SONORANT/$V_!$V
;   $SONORANT/u$SONORANT/!$V_$V

; Pre-PGmc

m/um/$C_#
m/um/#_$C
m/um/$C_$C

; n/m/_$LABIAL

; TODO implement transformation of Ø, i.e. insertion
; /u/#_$SONORANT$C

kʲ/kʷ/_w
gʲ/gʷ/_w
gʲʰ/gʷʰ/_w

[+$palatalized]/[-$palatalized]/_

[$LARYNGEAL]//#_$C
ʔ//_$V
χe/a/_
χʷe/o/_
e/eː/_ʔ
e/aː/_χ
e/oː/_χʷ
[$LARYNGEAL]/ə/$C_$C

χʷ/g/$SONORANT_w

; Early PGmc

j//_[$V-$high]#
w//_[$V-$high]#
[$V-$high]//_#

[$STOP-$voiced]/[+$fricative]/#_
[$STOP+$voiced]/[-$voiced]/#_
[$STOP+$aspirated]/[-$aspirated]/#_

[$STOP-$voiced]/[+$fricative]/$VGLIDE_
[$STOP+$voiced]/[-$voiced]/$VGLIDE_
[$STOP+$aspirated]/[-$aspirated]/$VGLIDE_

[+$fricative+$voiced]/[-$voiced]/[$V-$stressed]_
[-$stressed]/[+$stressed]/#($C)($C)_

gʷ/b/#_

o/a/_
a/ɑ/_

; Late PGmc

m/n/_#

ɑ/ɑ̃/_$NASAL#
[$NASAL]//_#    ; Why doesn't this one work?

m/n/_$DENTAL

ˈə/ˈɑ/$C_$C
ə//$C_$C

g/w/$VGLIDE_$VGLIDE

$V { a e i o u }

[$stressed] {
    ɑ => ˈɑ
    e => ˈe
    i => ˈi
    o => ˈo
    u => ˈu
    ə => ˈə
}

$VGLIDE {
    $V w j
    ə ɑ
}

$C {
    s
    $LARYNGEAL
    $STOP
    $SONORANT
    $fricative
}

$DENTAL { t d dʰ }

$LABIAL { m p b bʰ }

$SONORANT { m n l r w j }

$NASAL { m n }

$LARYNGEAL { ʔ χ χʷ }

$STOP {
    k kʲ kʷ p t
    g gʲ gʷ b d
    gʰ gʲʰ gʷʰ bʰ dʰ
}

[$high] {
    i u
}

[$voiced] {
    k => g
    gʰ
    x => ɣ
    kʲ => gʲ
    gʲʰ
    kʷ => gʷ
    gʷʰ
    p => b
    bʰ
    f => β
    t => d
    s => z
    dʰ
    d => ð
}

[$palatalized] {
    k => kʲ
    g => gʲ
    gʰ => gʲʰ
}

[$aspirated] {
    g => gʰ
    gʲ => gʲʰ
    gʷ => gʷʰ
    b => bʰ
    d => dʰ
}

[$fricative] {
    k => x
    kʷ => xʷ
    p => ɸ
    t => θ
    ɣ
    β
    ð
}
