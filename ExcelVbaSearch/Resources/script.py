
import os
import subprocess
import sys

def search_vba_in_excels(folder_path, search_string):
    matching_files = []  # Lista plików z podaną frazą
    search_string_lower = search_string.lower()  # Konwersja szukanego tekstu na małe litery

    for root, _, files in os.walk(folder_path):
        for file in files:
            if file.endswith(".xlsm"):
                file_path = os.path.join(root, file)
                try:
                    # Uruchomienie olevba bez hasła
                    result = subprocess.run(["python", "-m", "oletools.olevba", file_path],
                                            capture_output=True, text=True)
                    # Sprawdzenie, czy fraza jest w kodzie VBA (ignorując wielkość liter)
                    if search_string_lower in result.stdout.lower():
                        matching_files.append(file_path)
                except Exception as e:
                    print(f"Błąd podczas przetwarzania pliku {file_path}: {e}")

    return matching_files

# Odczytanie parametrów
if __name__ == "__main__":
    folder = sys.argv[1]
    search_string = sys.argv[2]

    matching_files = search_vba_in_excels(folder, search_string)
    for file in matching_files:
        print(file)
