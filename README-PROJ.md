# Projekt Inżynierski WikiGraph
Interaktywna wizualizacja artykułów Wikipedii w formie trójwymiarowego grafu ukazującego linki jako połączenia

**Unity version: 2018.1.9f2**

## Środowiska
- **LZWP**: Scena ``WIKIGRAPH``, ``GRAPH`` &rarr; ``Input Controller`` &rarr; ``Environment`` = ``Cave``
- **PC**: Scena ``WIKIGRAPH_NO_LZWP``, ``GRAPH`` &rarr; ``Input Controller`` &rarr; ``Environment`` = ``PC``

## Konfiguracja
- Wybierz obiekt ``GRAPH`` z okna hierarchii i znajdź ``Node Controller`` w Inspektorze
- Wybierz jedną z dostępnych opcji wikipedii (Data Pack)
- Ewentualnie dostosuj pola ``Max Node Limit`` oraz ``Node Starting Amount`` w ``Node Loader Controller``

#### Tworzenie własnych paczek danych
- Uruchom ``wikigraph-parser.sln``. Upewnij się, że w menadżerze NuGet są zainstalowane poniższe pakiety:
![screendependencies](https://user-images.githubusercontent.com/48009771/69915012-9f3e0a00-144a-11ea-9620-43406c004b87.png)
W razie występowania błędów w aplikacji w pierwszej kolejności dokonaj reinstalacji SQLite.Core.
- Zbuduj w trybie ``Release`` WikiGraph Parser. Po uruchomieniu aplikacji poczekaj, aż załaduje ona wszystkie dostępne wikipedie. Wybierz interesującą Cię paczkę danych oraz podaj ścieżkę do pobranego ``\wikigraph\Assets\StreamingAssets\DataFiles`` i naciśnij przycisk ``Start``. 
**UWAGA:** Pliki o rozmiarach przekraczających 1GB wymagają znacznej ilości pamięci RAM na komputerze.
![screenParser1](https://user-images.githubusercontent.com/48009771/69914843-08bd1900-1449-11ea-94fb-27cd524fa555.png)
- Poczekaj, aż program wygeneruje pliki do aplikacji. Ten etap może zająć od kilku sekund dla plików w rozmiarze <1MB, do kilku godzin dla plików wielogigabajtowych.
![screenParser2](https://user-images.githubusercontent.com/48009771/69914850-183c6200-1449-11ea-9530-826062956d84.png)
- Zakończenie przetwarzanie aplikacja zakomunikuje komunikatem ``Parsing data completed!`` w lewym dolnym rogu okna. Można teraz bezpiecznie zamknąć aplikację.
![screenParser3](https://user-images.githubusercontent.com/48009771/69914851-183c6200-1449-11ea-8c77-e69fc1508419.png)
- Wygenerowane pliki znajdują się we wskazanym katalogu z którego aplikacja WikiGraph będzie odczytywała dane. W przypadku wielokrotnego pobierania tych samych danych, rozróżniane są one na podstawie daty w nazwie folderu. WikiGraph używa najnowszej paczki z danymi.
![screenParser4](https://user-images.githubusercontent.com/48009771/69914852-18d4f880-1449-11ea-8456-78890abcbf94.png)

## Sterowanie
#### LZWP
![nowy_schemat_kontrolera](https://user-images.githubusercontent.com/48009771/69915461-72d8bc80-144f-11ea-8831-4a7917bc5143.png)

![schemat_kontroler_historia](https://user-images.githubusercontent.com/48009771/69915460-72d8bc80-144f-11ea-8cf3-aca93a8f631f.png)

#### PC
- Obracanie kamery: ``PPM`` i ruch myszą.
- Zaznaczanie i wybieranie węzłów: ``LPM``
- Wychodzenie z zaznczonego węzła do trybu latania w przestrzeni: ``ESC``
- Zmiana typu wyświetlanych połączeń gdy węzeł jest wybrany: ``M``
- Przewijanie połączeń: ``Z`` i ``X``
- Cofnięcie wykonanej akcji: ``E``, ponowienie: ``Q``
- Wyświetlenie pomocy: ``R``
- Otworzenie wyszukiwarki: ``F1``

Wszystkie powyższe ustawienia można dostosować w ``Input Controller(Script)`` w inspektorze po zaznaczeniu elementu ``GRAPH`` w hierarhii.

## Wygląd

![Screenshot_20191111_161452](https://user-images.githubusercontent.com/8643919/68598850-d6e71100-049f-11ea-8868-ad54bff2238c.png)

![Screenshot_20191111_161725](https://user-images.githubusercontent.com/8643919/68598852-d77fa780-049f-11ea-85a5-0154d9fc9119.png)

![Screenshot_20191111_162427](https://user-images.githubusercontent.com/8643919/68598853-d77fa780-049f-11ea-9ff4-b3a490e1ba41.png)
