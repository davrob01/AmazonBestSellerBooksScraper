using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmazonBestSellers
{
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

        public async Task ProcessCategory()
        {
            try
            {
                Category rootCategory = new Category(_rootCategoryName, _URL);

                List<Task<IEnumerable<Category>>> downloadTasks = new List<Task<IEnumerable<Category>>>();

                for(int page = 1; page <= 5; page++)
                {
                    if(page == 1)
                    {
                        downloadTasks.Add(rootCategory.RetrieveCategoryData(page));
                    }
                    else
                    {
                        downloadTasks.Add(rootCategory.RetrieveCategoryData(page, 0));
                        downloadTasks.Add(rootCategory.RetrieveCategoryData(page, 1));
                    }
                }
                _categories.Add(rootCategory);

                while(downloadTasks.Count > 0)
                {
                    Task<IEnumerable<Category>> firstFinishedTask = await Task.WhenAny(downloadTasks);

                    downloadTasks.Remove(firstFinishedTask);

                    var result = firstFinishedTask.Result;
                    firstFinishedTask.Dispose();

                    if(result != null)
                    {
                        var subCategories = result.ToList();

                        for (int page = 1; page <= 5; page++)
                        {
                            foreach (Category category in subCategories)
                            {
                                if (page == 1)
                                {
                                    downloadTasks.Add(category.RetrieveCategoryData(page));
                                }
                                else
                                {
                                    downloadTasks.Add(category.RetrieveCategoryData(page, 0));
                                    downloadTasks.Add(category.RetrieveCategoryData(page, 1));
                                }
                            }
                        }
                        _categories.AddRange(subCategories);
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                Logger.Log(string.Format("Error retrieving categories for {0}", _rootCategoryName), ex);
            }
        }
    }
}
