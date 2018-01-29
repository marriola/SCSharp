$VSHORT { ə ɑ a e i o u }
$VLONG { ɑː aː eː iː oː uː }
$V { $VSHORT $VLONG }

[$stressed] {
    ɑ => ˈɑ
    e => ˈe
    i => ˈi
    o => ˈo
    u => ˈu
    ə => ˈə
}

; TODO map sets
; $V -> &ː
;   produces aː eː iː oː uː

$VGLIDE {
    $V w j
    ə ɑ ɑː
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

[$mid] {
    e o
}

[$round] {
    o u
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
    θ => ð
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
    g => ɣ
    b => β
    d => ð
}
[+$stressed]/[-$stressed]/_

; TODO implement NOT nodes
;   $SONORANT/u$SONORANT/$V_!$V
;   $SONORANT/u$SONORANT/!$V_$V

; Pre-PGmc

m/um/(#|$C)_(#|$C)
n/un/(#|$C)_(#|$C)
l/ul/(#|$C)_(#|$C)
r/ur/(#|$C)_(#|$C)

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
eʔ/eː/_
eχ/ɑː/_
eχʷ/oː/_

[$LARYNGEAL]/ə/$C_$C

χʷ/g/$SONORANT_w

; Early PGmc

j//_[$V-$high]#
w//_[$V-$high]#
[$V-$high]//_#

; Grimm's law

[$STOP-$voiced]/[+$fricative]/(#|$VGLIDE)_
[$STOP+$voiced]/[-$voiced]/(#|$VGLIDE)_
[$STOP+$aspirated]/[-$aspirated]/(#|$VGLIDE)_

;[$STOP-$voiced]/[+$fricative]/$VGLIDE_
;[$STOP+$voiced]/[-$voiced]/$VGLIDE_
;[$STOP+$aspirated]/[-$aspirated]/$VGLIDE_

; Verner's law

[+$fricative]/[+$voiced]/(#|$C)[-$stressed]_
;[+$fricative]/[+$voiced]/$C[-$stressed]_

;;;;[+$fricative+$voiced]/[-$voiced]/[$V-$stressed]_

; Stress moves to initial syllable
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

e/i/!ˈ_
ji/i/_
ii/iː/_
