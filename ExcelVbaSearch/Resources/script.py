
import os
import subprocess
import sys

def search_vba_in_excels(folder_path, search_string):
    matching_files = []  # Lista plików z podaną frazą


    for root, _, files in os.walk(folder_path):
        for file in files:
            if file.endswith(".xlsm"):
                file_path = os.path.join(root, file)
                try:
                    # Uruchomienie olevba bez hasła
                    result = subprocess.run(["python", "-m", "oletools.olevba", file_path],
               capture_output=True, text=True)
                    # Sprawdzenie, czy fraza jest w kodzie VBA
                    if search_string in result.stdout:
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