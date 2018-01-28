[$STOP-$voiced]/[+$fricative]/$V_

; TODO implement OR nodes
;   $SONORANT/u$SONORANT/$V_(#|$C)
; TODO implement NOT nodes
;   $SONORANT/u$SONORANT/$V_!$V
;   $SONORANT/u$SONORANT/!$V_$V

m/um/$C_#
m/um/#_$C
m/um/$C_$C

n/m/_$LABIAL
m/n/_$DENTAL

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
χ/a/$C_$C
χʷ/o/$C_$C

χʷ/g/$SONORANT_w

j//_[$V-$high]#
w//_[$V-$high]#
[$V-$high]//_#

[$STOP-$voiced]/[+$fricative]/#_
[$STOP+$voiced]/[-$voiced]/#_
[$STOP+$aspirated]/[-$aspirated]/#_

[$STOP+$voiced]/[-$voiced]/$V_
[$STOP+$aspirated]/[-$aspirated]/$V_


$V { a e i o u }

$C {
    s
    $LARYNGEAL
    $STOP
    $SONORANT
    $fricative
}

$DENTAL { t d dʰ }

$LABIAL { m p b bʰ }

$SONORANT { m n l r }

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
    kʲ => gʲ
    gʲʰ
    kʷ => gʷ
    gʷʰ
    p => b
    bʰ
    t => d
    dʰ
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
}
