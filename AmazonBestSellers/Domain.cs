using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonBestSellers
{
    public class Domain
    {
        public string RootCategoryName { get; set; }
        public string URL { get; set; }
        public List<Category> Categories { get; set; }

        public Domain(string url, string rootCategoryName = "Books")
        {
            URL = url;
            RootCategoryName = rootCategoryName;
            Categories = new List<Category>();
        }

        public async Task ProcessCategory()
        {
            Category rootCategory = new Category(RootCategoryName, URL);

            List<Task<List<Category>>> downloadTasks = new List<Task<List<Category>>>();

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
            Categories.Add(rootCategory);

            while(downloadTasks.Count > 0)
            {
                Task<List<Category>> firstFinishedTask = await Task<List<Category>>.WhenAny(downloadTasks);

                downloadTasks.Remove(firstFinishedTask);

                List<Category> subCategories = firstFinishedTask.Result;

                if(subCategories.Count > 0)
                {
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
                    Categories.AddRange(subCategories);
                }
            }

        }
    }
}
