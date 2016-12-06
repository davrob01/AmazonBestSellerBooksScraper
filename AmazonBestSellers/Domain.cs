﻿/* Copyright (c) David T Robertson 2016 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonBestSellers
{
    /// <summary>
    /// Represents a domain, example: US books.
    /// </summary>
    public class Domain
    {
        private string _rootCategoryName;
        private string _URL;
        private List<Category> _categories;

        public List<Category> Categories 
        { 
            get 
            {
                return _categories;
            }
        }

        public Domain(string url, string rootCategoryName = "Books")
        {
            _rootCategoryName = rootCategoryName;
            _URL = url;
            _categories = new List<Category>();
        }

        /// <summary>
        /// Sets up and manages the tasks that process every page of every category in this domain.
        /// </summary>
        /// <returns>A task that completes when all categories have been processed.</returns>
        public async Task ProcessCategory()
        {
            try
            {
                Category rootCategory = new Category(_rootCategoryName, _URL);

                List<Task<IEnumerable<Category>>> downloadTasks = new List<Task<IEnumerable<Category>>>();

                // add a task for each page
                for(int page = 1; page <= 5; page++)
                {
                    if(page == 1)
                    {
                        downloadTasks.Add(rootCategory.RetrieveCategoryData(page));
                    }
                    else
                    {
                        if (_URL.Contains("www.amazon.co.jp")) // the Japan domain is the only domain that still uses the 'isAboveTheFold' query string for its ajax pages
                        {
                            downloadTasks.Add(rootCategory.RetrieveCategoryData(page, 0));
                            downloadTasks.Add(rootCategory.RetrieveCategoryData(page, 1));
                        }
                        else
                        {
                            downloadTasks.Add(rootCategory.RetrieveCategoryData(page));
                        }
                    }
                }
                _categories.Add(rootCategory);

                while(downloadTasks.Count > 0)
                {
                    Task<IEnumerable<Category>> firstFinishedTask = await Task.WhenAny(downloadTasks); // get a completed task

                    downloadTasks.Remove(firstFinishedTask); // remove tasks as they complete

                    firstFinishedTask.Dispose();

                    if(firstFinishedTask.Result != null)
                    {
                        // subcategories are found, add more tasks
                        var subCategories = firstFinishedTask.Result.ToList();

                        foreach (Category category in subCategories)
                        {
                            // add a task for each page
                            for (int page = 5; page >= 1; --page)
                            {
                                if (page == 1)
                                {
                                    downloadTasks.Add(category.RetrieveCategoryData(page));
                                }
                                else
                                {
                                    if (_URL.Contains("www.amazon.co.jp")) // the Japan domain is the only domain that still uses the 'isAboveTheFold' query string for its ajax pages
                                    {
                                        downloadTasks.Add(category.RetrieveCategoryData(page, 0));
                                        downloadTasks.Add(category.RetrieveCategoryData(page, 1));
                                    }
                                    else
                                    {
                                        downloadTasks.Add(category.RetrieveCategoryData(page));
                                    }
                                }
                            }
                        }
                        _categories.AddRange(subCategories); // update the list of subcategories in this domain
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw ex; // this may happen when the HtmlAgilityPack is not found
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("Error retrieving categories for {0}", _rootCategoryName), ex);
            }
        }
    }
}
