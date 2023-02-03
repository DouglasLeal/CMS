using AutoMapper;
using CMS.Models;
using CMS.ViewModels;

namespace CMS.AutoMapper
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, PostViewModel>();
            CreateMap<PostViewModel, Post>();
        }
    }
}
