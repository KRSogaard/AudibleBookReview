# Uages

There are currently no way to get personal information from Audible, such as what books you have purchase. We therefore need to scrape this information though you own browser.
The program can fetch audio book details once a list of owned books has been provided.

The import can take some time as the fetcher has been limited to 1 request per second to prevent issues from audible.

## Install WebScraper

1. Go to https://www.webscraper.io/ and install
2. Import the sitemap below
   > {"\_id":"audible","startUrl":["https://www.audible.com/library/audiobooks?pageSize=50"],"selectors":[{"id":"pagination","parentSelectors":["_root","pagination"],"paginationType":"auto","selector":".nextButton:not(.bc-button-disabled) a","type":"SelectorPagination"},{"id":"books","parentSelectors":["pagination"],"type":"SelectorElement","selector":"div.adbl-library-content-row","multiple":true,"delay":0},{"id":"title","parentSelectors":["books"],"type":"SelectorText","selector":"span.bc-size-headline3","multiple":false,"delay":0,"regex":""},{"id":"author","parentSelectors":["books"],"type":"SelectorText","selector":".authorLabel a","multiple":false,"delay":0,"regex":""},{"id":"book-link","parentSelectors":["books"],"type":"SelectorLink","selector":".bc-list-item > a","multiple":false,"delay":0}]}
3. Scrape the website (Make sure to be logged in)
4. Save the export

## Import

1. Open the app
2. Click the import button
