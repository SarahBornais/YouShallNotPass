def main():
    secret = parse_shallnotpass("hello this is shallnotpass!/shallnotpass 123/endshallnotpass/shallnotpass 456/endshallnotpass")
    print(secret)

def parse_shallnotpass(s):
    print(s)
    secret_start = s.find("/shallnotpass")
    secret_end = s.find("/endshallnotpass") 
    if (secret_start!=-1 and secret_end!=-1):
        protected_secret = protect_secrets(s[secret_start + 13:secret_end])
        return s[:secret_start] + protected_secret + parse_shallnotpass(s[secret_end + 16:])
    else:
        return ""

def protect_secrets(s):
    s.strip()
    return s + "Protected!!"


if __name__ == '__main__':
    main()