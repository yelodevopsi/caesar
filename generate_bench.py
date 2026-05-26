import random

words = [
    "the", "and", "for", "are", "but", "not", "you", "all", "can", "her",
    "was", "one", "our", "out", "day", "get", "has", "him", "his", "how",
    "man", "new", "now", "old", "see", "two", "way", "who", "did", "its",
    "let", "put", "say", "she", "too", "use", "try", "run", "big", "off",
    "hei", "jeg", "det", "den", "der", "til", "som", "med", "han", "hun",
    "ikke", "opp", "inn", "ved", "fra", "mot", "enn", "ble", "oss", "seg",
    "Æble", "Ørn", "Ånd", "Søen", "Bær", "Rødt", "Grøn", "Bæk", "Høj",
    "fjord", "øst", "vest", "sær", "Nørd", "Rød", "Blå", "Grå", "Tøj",
]

def gen_text(n_words: int) -> str:
    tokens = [random.choice(words) for _ in range(n_words)]
    # Sprinkle punctuation every 8-15 words
    i = random.randint(8, 15)
    while i < len(tokens):
        tokens[i] = tokens[i] + random.choice(['.', ',', '!', '?'])
        i += random.randint(8, 15)
    return ' '.join(tokens)

random.seed(42)

for label, count in [("100", 100), ("5000", 5000), ("100000", 100000), ("1000000", 1000000)]:
    path = f"bench_{label}.txt"
    with open(path, "w", encoding="utf-8") as f:
        f.write(gen_text(count))
    chars = len(open(path, encoding="utf-8").read())
    print(f"  {path}: {count} words, {chars:,} chars")
