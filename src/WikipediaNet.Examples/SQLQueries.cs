//select* from Articles where HitlerIndex = 2 and Title like N'% язык' order by WikiID
//delete from PreviousLevelPages where exists(select* from Articles where HitlerIndex = 2 and Title like N'% язык' and Articles.WikiID = PreviousLevelPages.PreviousLevelWikiID)

//delete from Articles where WikiID = 638
//delete from PreviousLevelPages where PreviousLevelWikiID = 638

//select* from Articles where not exists(select* from PreviousLevelPages where PreviousLevelPages.WikiID = Articles.WikiID) and HitlerIndex = 2

//select dbo.PreviousLevelPages.*, dbo.Articles.Title, dbo.Articles.HitlerIndex from dbo.PreviousLevelPages
//left outer join dbo.Articles on dbo.Articles.WikiID = dbo.PreviousLevelPages.PreviousLevelWikiID where dbo.PreviousLevelPages.WikiID = 1850272




//select count(*) from MissingArticles
//select count(*) from MissingArticles where Title like N'%:%' 
//select count(*) from MissingArticles where Title not like N'%:%' 

//select* from MissingArticles where  Title NOT like N'%:%' order by WikiID

//select* from MissingArticles where not exists(select* from ToDelete where toDelete.WikiID = MissingArticles.WikiID) and Title not like N'%:%' 
//and Title not like N'%(значения)%' order by WikiID


//select* from ToDelete where Redirect = 0 and not exists(select* from Articles where Articles.WikiID = ToDelete.WikiID)

//select* from ToDelete
//where WikiID = 75935
//--where Redirect = 1
//--where Title = N'Унарная операция'
//order by WikiID desc

//select* from MissingArticles where WikiID = 216767 and Title NOT like N'%:%' order by WikiID

//select* from ToDelete where exists(select* from Articles where Articles.WikiID = ToDelete.WikiID and Articles.HitlerIndex = -1)
//select* from Articles where HitlerIndex = -1