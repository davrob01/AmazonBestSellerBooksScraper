****************************************
*                                      *
*   Amazon Best Seller Books Scraper   *
*                                      *
* Copyright (c) David T Robertson 2016 *
*                                      *
****************************************

-------------------------------------
DESCRIPTION:
-------------------------------------

This program scrapes Amazon to get all the ISBNs of the Best Selling books in EVERY category and every subcategory.

The following websites are included in the scraping process:

http://www.amazon.com/best-sellers-books-Amazon/zgbs/books/
http://www.amazon.co.jp/gp/bestsellers/english-books/
http://www.amazon.co.uk/gp/bestsellers/books/
http://www.amazon.it/gp/bestsellers/books/
http://www.amazon.fr/gp/bestsellers/english-books/
http://www.amazon.de/gp/bestsellers/books-intl-de/
http://www.amazon.es/gp/bestsellers/foreign-books/

-------------------------------------
SYSTEM REQUIREMENTS:
-------------------------------------

Windows 7 64 bit or better

.NET Framework ver 4.5 installed

                  Minimum                Recommended
                  =======                ===========
Memory:           2 GB Available         4 GB Available is recommended
CPU:              Core 2 duo             Intel i5 or better is recommended
Bandwidth:        At least 500 Kbps      10 Mbps is recommended
Connection: 	  WIRED ETHERNET
HDD:              500 MB free

-------------------------------------
INSTRUCTIONS:
-------------------------------------

1. Extract the zip file to a new folder
2. Open the folder and Run the file AmazonBestSellerBooksScraper
3. A new window should appear. Click "Start" button to begin the process.
4. Progress will be updated throughout. Books added should constantly be increasing.
5. Let the program run until completion.
6. A successful run will say "Process Complete!" and about 2 million books should be added.
7. See "Results" folder inside the same folder that contains AmazonBestSellerBooks

Note: Depending on your internet connection, and system specs, the process may take between 1 to 3 hours.

* You can optionally do a quick test run via the "Test Run" button. About 104000 books are scraped in a successful test run. This test serves to verify you can run the process and also gives you an example of results. 

-------------------------------------
OUTPUT:
-------------------------------------

See "RESULTS" folder, created in the current folder after running.

A text file with all the ISBNs, unordered, one per line. This is always created. It is marked by date and time- this way future scrapes will not overwrite previous result files.

Example of file contents:

8856653141
8867315196
B016P0AYC2
8817085006
8863862192
B0064BV4RW

------------------------------------------------
OPTIONAL OUTPUT - MORE DETAIL - OPEN WITH EXCEL:
------------------------------------------------

By selecting the corresponding check box, additional output files are created, one per domain, in the CSV Format (Comma-Separated-Value). They contain the following: 

Book category, Rank, ISBN, price and title is displayed. One record per line.

This file was designed for opening in Microsoft Excel.

With this option checked, the scraping process will take a bit longer and consume more system memory. See System Requirements.

Example when viewing in Excel:

Category                       Rank	ISBN        Price   Title
US Books > Arts & Photography	1	0995056706  $7.45   Quieting Your Heart: 6-Month Bible-St...
US Books > Arts & Photography	2	1522864741  $8.01   Calm the F*ck Down: An Irreverent Adu...
US Books > Arts & Photography	3	0996275460  $7.24   Adult Coloring Books: A Coloring Book...
US Books > Arts & Photography	4	0399542299  $9.29   Doctor Who Coloring Book
US Books > Arts & Photography	5	B007V65PLA  $12.99  Argo: How the CIA and Hollywood Pulle...
US Books > Arts & Photography	6	1516866746  $5.31   Adult Coloring Book: Butterflies and...
	

-------------------------------------
ADVANCED OPTIONS:
-------------------------------------

A) Connections_Per_Domain

If you know you have a very fast internet connection of 20 Mbps or greater. You may optionally increase the number of connection per domain (website). This setting is found in the .config file. The default setting is 10. Max is 100. 

Increasing this number may speed up the process. However, more bandwidth may be consumed, so if you expect to use your internet connection while the process is running, it may be slow (you may even want to reduce this number in that case).

B) Automatic Start and Scheduling

The command line option "autostart" bypasses the Start button and will run the process immediately. This can be used in conjuction will the Windows Task Scheduler if you would like to run this process daily, or at a set time.

C) Error Logging

An error log file is always created after every execution. Unless there are no errors.

-------------------------------------
Frequently Asked Questions
-------------------------------------

1. I selected for the detailed output. Why do some category have less than 100 books?

A: If you browse to that category on Amazon's websites you will most likely find that some categories do in fact have less than 100 books.

	i. I checked Amazon and the amount of books in that category is way different.
	
	A: Check the log.txt file. It is possible there was an error downloading one of the pages.

2. Regarding detailed output, why are the results divided into separate files by domain? Why not just make one big file?

A: MS Excel cannot open a file that has more than 1,048,576 rows. Also the file would be very large and take time to open.

-------------------------------------
SUPPORT AND FEEDBACK
-------------------------------------

Please send any feedback or questions to drobsoftware@gmail.com




