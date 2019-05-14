---
output:
  html_document: default
  pdf_document: default
---
# Service Center (Codename: Concordia)

## Einleitung

Willkommen.

### Visual Studio Solution

Die Lösung enthält 4 Projekte:

- **Business Objects**: Allgemeine Objekte
- **Web Application**: MVC .NET Core Front End Web Applikation.  
- **Web Service**: MVC .NET Core API Applikation.
- **Report Project**: SQL Server Reporting Services Projekt

### Links

**Live Applikation**:
<https://live.local/WebApplication/ServiceCenter/>

**Live API**:
<https://live.local/WebService/api>

**Live Reporting Services**:
<http://live/Reports/Pages/Folder.aspx?ItemPath=%2fService+Center>

**Pfad zu IIS:**
[\\\\live\c$\inetpub\wwwroot_ServiceCenter](\\live\c$\inetpub\wwwroot_ServiceCenter)

**Jira URL:**
<https://xxx.atlassian.net/browse/BND-1034>

### Voraussetzungen

- [.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2)
- [Microsoft Reporting Services Projects](https://marketplace.visualstudio.com/items?itemName=ProBITools.MicrosoftReportProjectsforVisualStudio)

### Nützliche Tools

- [Markdown Editor](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor#review-details)
- [Postman](https://www.getpostman.com/)

### Lösung einrichten

Man muss die Visual-Studio-Lösung zuerst einrichten, um mehrere Projekte im Debug-Modus gleichzeitig zu starten. Um das zu machen:

- Mit der rechten Maustaste auf den Lösungsknoten (ganz oben in der Lösung) klicken.
- Menüpunkt auswählen **Startprojekte festlegen**
- Wählen Sie das Optionsfeld **Mehrere Startprojekte**
- **Web Application** und **Web Service** auf **Start** setzen
- **Business Objects** auf **Keine** lassen

Man kann mit dem SQL Server Test Server (SQLTEST) mit einem SSPI-Context nicht verbinden (Integrated
Security) daher muss man einen SQL-Benutzer auf SQLTEST einrichten.  Um diese Anmeldeinformation
nutzen zu können, ohne dass der Benutzername bzw. Kennwort eingecheckt werden müssen, kann
man den unsicheren "Secure Store" nutzen.  Die folgenden Befehle sollten aufgeführt werden:

    dotnet user-secrets set "MAINDB:ConnectionString:UserId" "YourUserId"
    dotnet user-secrets set "MAINDB:ConnectionString:Password" "YourPassword"

Diese Information wird im WebServices Project Startup / ConfigureServices benutzt werden, um einen
Connection String zu bauen.

### Hintergrund

Die ursprüngliche Idee dieses Projekts war eine *Universallösung* zu starten. Deshalb
wurde die Lösung in 3 Projekt unterteilt: **Web Application**, **Web-API-Service**
und **Business Objects**. Das Web Application hat Platzhalter für zukünftige
Entwicklung. Der Ausgangspunkt für das API-Projekt war für einen Webdienst für
alle Anwendungen, um Geschäftslogik zu zentralisieren und zu kapseln. Die
Business Objects sollten irgendwann ein NuGet-Paket werden.  

Leider konnte diese Vision aus Zeitgründen nicht realisiert werden und
die Hoffnung, dass diese Anwendung der 1. Schritt Xxxxx
aufzulösen wird, wird nicht realisierbar.  Weiterentwicklung um
Bestellung bzw. Kunden zu bearbeiten wird nicht in dieser Anwendung möglich.

Das Endergebnis ist ein interner Webshop.

## Datenbank

Die Schnittstellentabellen im xxxxxOS sind:

- dbo.einlesen_Kopfdaten
- dbo.einlesen_Rechnungsadresse
- dbo.einlesen_Lieferadresse
- dbo.einlesen_Artikel

Es wird in diese Tabelle geschrieben um eine Bestellung abzugeben.

Die entsprechenden Tabellen im Schema **Service Center** in der Datenbank MAINDB sind:

- ServiceCenter.Invoice
- ServiceCenter.InvoiceBillingAddress
- ServiceCenter.InvoiceDeliveryAddress
- ServiceCenter.InvoiceItem

Diese Tabelle sind fast eine 1-1 Spiegelung von den xxxxxOS Tabellen (mit kleinen Abweichungen).

Des Weiteren unterstützen diese Tabelle Funktionalität:

- ServiceCenter.CustomerComment
- ServiceCenter.CustomerMapping

Diese Tabelle unterstützen die Benutzeroberfläche

- ServiceCenter.DeliveryPaymentButton
- ServiceCenter.PznAvailabilityTypeOverride
- ServiceCenter.CustomerWarning

Diese Tabelle unterstützen die Weiterbearbeitung von Bestellungen

- ServiceCenter.InvoiceStatus

## Business Objects

Das Projekt **Business Object** war ursprünglich eine allgemeine Extrahierung
der Geschäftslogik. Es ist jetzt nur ein einfaches Modell der 'Einlese'-Tabellen
und kann wahrscheinlich ins Web-Application-Projekt bewegt werden.

- ⇡ **Customer**: Ist eine Rechnungsadresse in einer Kundengruppe
- ⇡ **BillingAddress**: Ist eine Lieferadresse mit Telefonnummer
- ⇡ **DeliveryAddress**: Ist eine Adresse mit einer Bestellungsnummer
- ↥ **AddressBase**: Basisklasse

Hilfsobjekte und Überreste von der Idee von globalen Objekten für alle Projekt sind selbsterklärend:

- Affiliate
- Article
- ArticlePictureTemplate (Bilder von den Webshops)

## Web Application

Die **Web Application** ist ein MVC-Projekt mit Core 2.2.  Die
Designphilosophie war ein Call-Center-Mitarbeiter bei einem Call
zu begleiten und nicht ein allsehendes und allwissendes Hundefrühstück
wie Xxxxx.

**Screens**:

<center>
<strong>
CustomerSearch (Kundensuche)
<br/>
↓
<br/>
Invoice (Beleg)
<br/>
↓
<br/>
DeliveryPayment (Lieferung, Zahlung)
<br/>
↓
<br/>
Summary (Zusammenfassung)
<br/>
↓
<br/>
Confirmation (Bestätigung)
</strong>
</center>
<br/>

Man kann auch (fast) zu jeder Zeit zum **Comment (Kommentar)** und **CustomerEdit (Kundenbearbeitung)** Screens navigieren.

**Buttons – Allgemeines Design**:

- **Grüne Buttons**: Actions um eine Bestellung weiterzubringen
- **Blaue Buttons**: Mögliche zusätzliche Actions bei einer Bestellung
- **Rote Buttons**: Abbrechen
- **Pfeile**: Nächster bzw. vorheriger Schritt
- **Bleistift**: Kommentar
- **Benutzer**: Kundendaten bearbeiten
- **Floppy Disk**: Daten speichern (d.h. Daten ist noch nicht gespeichert)

### Customer Search Screen

Selbsterklärend.  

### Invoice Screen

Auf diesem Screen wird eine Bestellung aufgenommen.  Man sucht nach
Artikelnamen bzw. PZN.  Die Eingabetaste bzw. Lupenbutton betätigen
die Suche.

Buttons:

- **Lupe**: Suche nach PZN bzw. Artikelnamen (nicht Herstellername eines Artikels)
- **Liste**: Die letzten Bestellungen
- **Herzchen**: Häufig bestellte Artikel

Suchergebnis Buttons:

- **Auge**: Zeigt das Produktbild (direkte Link mit der Website)
- **Link**: Öffnet die Produktseite bei der Website
- **Eurozeichen**: Preisvergleich (Noch nicht implementiert)
- **Plus**: fügt einen Artikel zu einer Bestellung

Die Artikel werden sofort in der **ServiceCenter.InvoiceItem** Tabelle
über die Webmethode **AddInvoiceItem** hinzugefügt.  Diese Webmethode
berechnet auch evtl. auch der Rabatt und fügt dieser als eine Position hinzu.

### DeliveryPayment Screen

Auf diesem Screen werden die zusätzlichen Daten für eine Bestellung
aufgenommen.  

**Linke Seite**:

- schon benutzte Adressen werden aufgelistet und kann angeklickt werden
- eine neue Lieferadresse kann eingetragen werden

**Rechte Seite**:

- **Zahlungsart**.  Der Plusbutton bei 'Lastschrift' fügt die letztbenutzte Lastschriftdaten hinzu.
- **Lieferart**.  Liefergebühren ändern sich nach Zahlungsart, daher eine neue Zahlart auszuwählen, setzt die Lieferart zurück
- **Zusätzliche Buttons**: Diese kommen aus der Tabelle **ServiceCenter.DeliveryPaymentButton**.

### Summary Screen

Zeigt die ganze Bestellung nochmal zur Bestätigung und führte eine Validierung durch.

Wenn man den Häkchen Button klickt, wird die Bestellung mit dem StoredProc `usp_InvoiceInsertIntoEinlesen` in die Einlese Tabelle kopiert und
die InvoiceId Spalte befüllt.  

### Comment Screen

Man kann Kommentar für einen schon existierenden Kunden abgeben.  Das Kommentar wird in der Tabelle `ServiceCenter.CustomerComment`
gespeichert und mit dem StoredProc `usp_InsertCommentInCustomer` in xxxxxOS hinzugefügt.

### Web Application Bemerkungen

- Das Projekt wird in Areas aufgeteilt (Einkauf, Feed, Middleware, ServiceCenter).   Diese können vermutlich gelöscht werden.
- Es gibt einen **DefaultController** in jedem Area um Requests weiterzuleiten.
- ServiceCenter wird aufgeteilt:
  - **Controllers**: Der Ordner hat ein Unteordner **Order** in dem die zusammenpassenden Partial-Classes gespeichert werden
  - **Data**: Die Methoden leiten die Anfragen an den Webdienst einfach weiter.  Die Partial-Classes liegen im Ordner **Order**
  - **Models**: Aufgeteilt in **Views**, **PartialViews**, **Objects** (Hilfsobjekte)
  - **Views**
- Es gibt ein **BaseController**.  Deises Basisobjekt hat das Attribut **[Authorize(Policy = "ServiceCenterUser")]**
  - Policies können im Projekt hier gefunden werden: **\WebApplication\Policies\ServiceCenterUser**
  - In der Methode **HandleRequirementAsync** kann man die Entsprechende AD-Role hinzufügen
- Es gibt einen **Resources** Ordner, wo die Resource-Dateien sind.  Der Pfad zum Resource-Datei muss genau den Pfad übereinstimmen, für das das Resource-Datei übersetzt.

## Web Service

Dieses Projekt ist eine MVC .NET Core API Applikation.  Alle Data Access fürs Projekt wird über diese Webmethoden
gemacht.

Entity Framework Core wird für den Datenbankzugang eingesetzt und die Einstellungen können in appsettings.json
(und dementsprechend Development bzw. Production) gefunden.  

Im der Live-Umbegung läuft der Dienst im Kontext von **live\ccwebapp**.  Datenbankzugang
läuft mit Intergrated Security von diesem Benutzter.  

Ursprünglich sollte dieses Projekt eine saubere Trennung zwischen der Datenbank und den Konsumenten der Dienste.  Inzwischen ist
das Ziel auf das ServiceCenter verschrumpft.  Eventuell könnte dieses Projekt aufgelöst und ins Web Application
Projekt hereingezogen.

### Controllers

Der Controllers Ordner hat 2 Unterordner:

- **Common**: Diese Controllers bieten Methoden an, die vielleicht nützlich für weitere Anwendungen sind.  Sie nutzten den Db-Kontext **MAINDBContext**.
- **WebApplication**: Dieser Controller bietet Methoden ausschließlich für das ServerCenter an.  Sie nutzt den Db-Kontext **ServiceCenterContext**.

### Models

Der Models Ordner hat 3 Unterorndner:

- **MAINDB**: Inhalt wird vom Befehl `dotnet ef dbcontext scaffold` generiert.  Klassen sind Tabellen aus dem **dbo** Schema.  Du solltest keine Änderung hier direkt machen.
- **ServiceCenterContext**: Inhalt wird vom Befehl `dotnet ef dbcontext scaffold` generiert.  Klassen sind Tabellen aus dem **ServiceCenter** Schema.  Du solltest keine Änderung hier direkt machen.
- **Extensions**: Beinhaltet Erweiterungen, Hilfsmodellen und Hilfsklassen.

### Kontextgenerierung

Anweisungen wie man eine Tabelle hinzufügen kann, kann man im **\WebService\Models\Extensions\MAINDBContext.cs** finden.

Man kann den Kontext für **ServiceCenterContext** ohne weiteres generieren.

Es gibt allerdings eine Partial-Class für **MAINDBContext**.  Diese Partial-Class enthält
Erweiterungen und Korrekturen.  Wenn man MAINDBContext neu generiert, gibt es ein paar Sachen zu bemerken:

- In /Models/MAINDB/MAINDBContext '`override void OnModelCreating`' auf '`OnModelCreatingAutoGenerated`' ändern
- In /Models/MAINDB/MAINDBContext '`override void OnConfiguring`' löschen
- /Models/MAINDB/KundenGruppe löschen.  Ein ausgebessertes Objekt gibt es in /Models/Extensions

### Web Service Bemerkungen

- `CustomerId` referenziert **CustomerId** im Schema **ServiceCenter**; `KundenNr` referenziert PKKundenId, FKKundenId bzw. CusId usw. in den dbo MAINDB Tabellen.
- Alle Methoden werden mit den Parametern **AffiliateId** und **CustomerId** aufgerufen.  Bei `POST` Methoden wird **UserName** auch übergeben.
  - Nachhinein wäre es besser gewesen, wenn ich die InvoicePk für alle Methoden bentzt hätte. Leider hatte ich keine Zeit dies auszubessern.  Sorry.
- Invoices die noch in der Bearbeitung sind haben InvoiceId 0.  Wenn das Invoice übergeben wird, bekommt es einen Wert.  Daher gibt es keinen Status-Feld
- Es gibt einen SQL Server Agent Job **Service Center App Cleanup**, der abgebrochene Bestellungen aufräumt.

## Report Project

Berichte werden auf SQL Server Reporting Services zur Verfügung gestellt:

<http://live/Reports/Pages/Folder.aspx?ItemPath=%2fService+Center>

Die Berichte haben auch eine Abonnement, welches jeden Abend ein PDF ablegt:

[\\\\fileserver\Dokumente\Service Center Reports](\\fileserver\Dokumente\Service Center Reports)
