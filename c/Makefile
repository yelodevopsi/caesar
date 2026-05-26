# Makefile (nmake syntax) for the C Caesar cipher implementation
# Run from a Visual Studio Developer Command Prompt:
#   nmake          — build caesar.exe
#   nmake test     — build & run tests
#   nmake clean    — remove build artefacts

CC     = cl
CFLAGS = /O2 /W3 /std:c17 /nologo

all: caesar.exe

caesar.exe: caesar.c
	$(CC) $(CFLAGS) /Fe:caesar.exe caesar.c

test: caesar.c caesar_test.c
	$(CC) $(CFLAGS) /DTEST /Fe:caesar_test.exe caesar.c caesar_test.c
	.\caesar_test.exe

clean:
	-del /Q caesar.exe caesar_test.exe *.obj 2>NUL
