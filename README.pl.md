# Route Tracker Commander DLC

[English](README.md) | **Polski**

To samodzielne repozytorium zawiera wyłącznie źródła dodatku, projekt kompilacji, testy i dokumentację. Nie zawiera drzewa źródeł głównego programu EDDiscovery.

Pobieranie: [Route Tracker Commander 1.1.3](https://github.com/SirAdams/RouteTrackerCommander/releases/tag/v1.1.3).

Dodatek DLL do EDDiscovery dodaje osobny panel **Route Tracker — Commander**. Nie wymaga podmiany pliku EDDiscovery.exe ani bibliotek programu.

Dostępne są dwie osobne paczki:

- **EDD19.1.11**: przetestowana z oryginalnym wydaniem EDDiscovery 19.1.11.0.
- **EDD20.x**: przetestowana z niezmodyfikowanym masterem f9af793b8e4055cc384868fc902efd1c2900a8f0, wersja programu 20.0.0.0. Kolejne wersje 20.x mogą zmienić interfejs paneli i wymagać aktualizacji DLL.

Obie paczki zawierają plik RouteTrackerCommander.dll. Zainstaluj wyłącznie wariant pasujący do Twojego EDDiscovery. Dodatek sprawdza wersję programu podczas inicjalizacji.

## Możliwości

- Korzysta z oryginalnej ikony Route Tracker w selektorze paneli EDDiscovery.

- Ikona kopiowania na pasku panelu pozwala ponownie skopiować widoczny cel trasy po nadpisaniu schowka przez inną aplikację. Działa także przy wyłączonym automatycznym kopiowaniu, nie przesuwa postępu trasy i jest nieaktywna, gdy nie ma celu.

- Wybrana zapisana trasa lub Nav Route, postęp trasy i opcje panelu są zapisywane osobno dla każdego komandera.
- Zmiana komandera przywraca jego trasę i ustawienia. Komander bez zapisanej trasy widzi pusty panel.
- Nieaktualne wyniki skanowania asynchronicznego i opóźnione działania menu nie nadpisują stanu innego komandera.
- Aktualizacja trasy korzysta z najnowszego wpisu w historii wybranego komandera, niezależnie od położenia kursora historii.
- Dodatek wykorzystuje istniejące trasy, historię, dane statku i FSD, bazę użytkownika, motyw i układ paneli EDDiscovery. Nie wymaga osobnej bazy układów.
- Domyślny język interfejsu jest angielski. Istniejące tłumaczenia programu są używane tam, gdzie są dostępne. Dodatek nie podmienia słowników EDDiscovery.

Definicje zapisanych tras pozostają wspólne. Wybrana trasa i ustawienia jej śledzenia są rozdzielone według komandera oraz instancji panelu/profilu interfejsu. Wybierz trasę dla każdego komandera; ustawienia standardowego Route Trackera nie są przenoszone automatycznie.

## Instalacja

1. Zamknij EDDiscovery.
2. Pobierz ZIP odpowiadający wersji EDDiscovery i rozpakuj go.
3. Skopiuj RouteTrackerCommander.dll do `%LOCALAPPDATA%\EDDiscovery\DLL`. Jeśli korzystasz z niestandardowego katalogu danych aplikacji, użyj jego podkatalogu DLL. Możesz również uruchomić dołączony Install.ps1, podając `-AppDataDirectory` dla niestandardowego katalogu danych.
4. Jeśli Windows oznaczy pobraną DLL jako zablokowaną, odblokuj ją we właściwościach pliku.
5. Uruchom EDDiscovery i zezwól na dodatek, gdy pojawi się pytanie. Uprawnienia DLL można też ustawić w Settings.
6. Dodaj zakładkę/panel **Route Tracker — Commander** z listy paneli EDDiscovery. Wybierz komandera, a następnie trasę i jej ustawienia.
7. Automatyczne kopiowanie do schowka lub ustawianie celu włącz tylko w jednym trackerze. Wyłącz te opcje w pozostałych panelach tras, aby nie nadpisywały sobie celu.

Panel DLC musi pozostać otwarty, aby śledzić bieżące skoki; możesz przełączyć się na inną zakładkę. Zamknięcie lub usunięcie panelu zatrzymuje jego śledzenie do ponownego otwarcia. Zdarzenia i historia są dostarczane przez EDDiscovery.

Nie kopiuj katalogów host/reference/test ze źródeł do EDDiscovery. Instalowany jest tylko RouteTrackerCommander.dll. Paczki nie zawierają pełnego programu EDDiscovery.

## Sprawdzanie aktualizacji

Ikona odświeżania na pasku panelu sprawdza wydania tego repozytorium na GitHubie i szuka nowszego ZIP-a odpowiadającego wersji EDDiscovery. Sprawdzanie działa w tle przy otwieraniu panelu oraz co 12 godzin, gdy panel pozostaje otwarty. Zapytania są współdzielone między panelami i mają limit czasu 15 sekund. Błąd połączenia nie przerywa śledzenia trasy.

Gdy dostępne jest nowsze pasujące wydanie, na ikonie pojawia się wykrzyknik. Kliknij ją, aby zobaczyć wersję i otworzyć stronę wydania. Jeśli aktualizacja nie jest jeszcze znana, kliknięcie ponawia sprawdzanie. Prawym przyciskiem myszy wybierz **Check now** albo wyłącz **Automatically check for updates**. Ta opcja jest wspólna dla komanderów i zapisywana w bazie użytkownika EDDiscovery.

Wydania testowe są uwzględniane i oznaczane. Nowsze wydanie bez paczki dla właściwej wersji programu jest pomijane. Sprawdzanie korzysta z publicznego API GitHuba, bez wysyłania danych komandera, dzienników ani tras; GitHub nadal otrzymuje zwykłe informacje o połączeniu, takie jak adres IP. Logowanie ani token GitHuba nie są wymagane.

Ta funkcja sprawdza dostępność i prowadzi do pobierania; nie pobiera, nie instaluje ani nie podmienia DLL automatycznie. Zamknij EDDiscovery przed instalacją aktualizacji. Główny program nie wymaga zmian.

Logika wyboru wydań ma 19 dodatkowych sprawdzeń offline w tests/UpdateTests.cs. Skompiluj test razem z shared/ReleaseChecker.cs, z referencjami System.Net.Http.dll i System.Web.Extensions.dll. Test nie łączy się z GitHubem.

## Aktualizacja i usuwanie

Przed podmianą DLL na właściwą nową wersję zamknij EDDiscovery. Instalator tworzy kopię poprzedniej DLL. Zapisane ustawienia pozostają w bazie użytkownika EDDiscovery.

Aby usunąć dodatek, zamknij EDDiscovery i przenieś RouteTrackerCommander.dll poza katalog DLL. Standardowy Route Tracker pozostaje dostępny. Przy zmianie EDDiscovery z wersji 19 na 20 podmień DLL na wariant z paczki EDD20.x.

## Kompilacja

Użyj Visual Studio 2022 / MSBuild z narzędziami deweloperskimi .NET Framework 4.8. Jako HostDir podaj odpowiednią niezmodyfikowaną instalację EDDiscovery. Uruchom polecenia w katalogu projektu:

```powershell
MSBuild RouteTrackerCommander.csproj /t:Rebuild /p:TargetHost=edd19 /p:HostDir="C:\Program Files\EDDiscovery"
MSBuild RouteTrackerCommander.csproj /t:Rebuild /p:TargetHost=edd20 /p:HostDir="C:\Program Files\EDDiscovery20"
```

Źródła są rozdzielone na edd19/src i edd20/src, ponieważ EDDiscovery 20 zmienia implementację Surveyor/Route Tracker oraz interfejsy tłumaczeń. Wynikowe DLL znajdują się w bin/edd19 lub bin/edd20. Nie używaj ich zamiennie.

## Testy

Każdy wariant przeszedł 65 sprawdzeń, obejmujących rozdzielenie komanderów, pusty widok, postęp trasy, ponowne otwieranie, nieprawidłowe pozycje, wywołania podczas startu, wyładowanie i ponowne załadowanie DLL, nieudaną inicjalizację oraz odrzucenie niewłaściwej wersji programu. Oba warianty sprawdzono również podczas rzeczywistego uruchomienia niezmodyfikowanego EDDiscovery, inicjalizacji DLL, otwierania panelu i zamykania programu, na osobnych bazach testowych. Bieżące skoki w grze nadal wymagają sprawdzenia przez użytkowników.

Testy znajdują się w katalogu tests każdego wariantu oraz w tests/StartupSmoke.cs. Nie korzystają z bazy użytkownika. Paczki do pobrania nie zawierają programów testowych ani baz danych.

## Licencja i pochodzenie

Apache License 2.0; zobacz LICENSE.md. Kod panelu wywodzi się z EDDiscovery i zachowuje oryginalne informacje o prawach autorskich. Wariant dla EDDiscovery 19 bazuje na Release_19.1.11 (a3cbe2190779ea4dcd186db65aeb3f4fa1baccb9); wariant 20 na masterze f9af793b8e4055cc384868fc902efd1c2900a8f0. Jest to dodatek społeczności, a nie oficjalne wydanie EDDiscovery.
