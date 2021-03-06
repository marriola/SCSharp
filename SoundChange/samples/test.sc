; Rounded consonant becomes unrounded preceding an unrounded vowel
[$C+$round]/[-$round]/_[$V-$round]

; Stress moves to initial syllable
[+$stressed]/[-$stressed]/_
[-$stressed]/[+$stressed]/#($C)($C)_

; /r/ becomes voiceless when preceding a voiceless consonant in coda position
r/[-$voiced]/[$C-$voiced]_#

; /r/ becomes voiceless when following a voiceless consonant in onset position
r/[-$voiced]/#[-$voiced]_

; /i/ is elided when preceded by a syllable and followed by a syllable with /i/ as the nucleus
i//$V$C_($C)$C.i

ea/ɛː/_
ai/eː/_
au/oː/_

$V { ɑ a e i o ø u y ˈɑ ˈa ˈe ˈi ˈo ˈø ˈu ˈy }

$C { k kʷ p t g gʷ b d x f θ s z m n w j ʔ r }

[$stressed] {
  a => ˈa
  e => ˈe
  i => ˈi
  o => ˈo
  u => ˈu
}

[$voiced] {
  k => g
  kʷ => gʷ
  p => b
  t => d
  m̥ => m
  n̥ => n
  r̥ => r
  s => z
}

[$back] {
  ɑ
  o
  u
}

[$round] {
  o
  u
  ø
  y
  k => kʷ
  g => gʷ
  t => p
  d => b
}

[$front] {
  a
  e
  i
  ø
  y
}
