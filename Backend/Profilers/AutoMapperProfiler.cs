using AutoMapper;
using Backend.Models.Domains;
using Backend.Models.DTOs;
using Backend.Utils;

namespace Backend.Profilers;

public class AutoMapperProfiler : Profile
{
    public AutoMapperProfiler()
    {
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<User, AuthenticationDto>()
            .ForMember(dest => dest.PasswordMd5, opt => opt.Ignore())
            .ForMember(dest => dest.Token, opt => opt.Ignore());
        CreateMap<AuthenticationDto, User>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedUsers, opt => opt.Ignore())
            .ForMember(dest => dest.Quizzes, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedQuizzes, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionGroups, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedQuestionGroups, opt => opt.Ignore())
            .ForMember(dest => dest.Questions, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentGames, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedCurrentGames, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentGameUser, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentGameUsers, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedCurrentGameUsers, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentGameQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedCurrentGameQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.AnsweredCurrentGameQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAnsweredCurrentGameQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.QuestionGroupQuizzes, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedQuestionGroupQuizzes, opt => opt.Ignore())
            .ForMember(dest => dest.PreviousGames, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedPreviousGames, opt => opt.Ignore())
            .ForMember(dest => dest.PreviousGameUsers, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedPreviousGameUsers, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedByUser, opt => opt.Ignore());
        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.QuestionGroupName, opt => opt.MapFrom(src => src.QuestionGroup.Name))
            .ReverseMap();
        CreateMap<QuestionGroup, QuestionGroupDto>().ReverseMap();
        CreateMap<Quiz, QuizDto>().ReverseMap();
        CreateMap<CurrentGameQuestion, CurrentGameQuestionDto>()
            .ForMember(dest => dest.AnswerHint, opt => opt.MapFrom(src => src.Question != null ? Utility.GenerateAnswerHint(src.Question.Answer) : null))
            .ReverseMap();
        CreateMap<CurrentGame, CurrentGameDto>().ReverseMap();
        CreateMap<CurrentGameUser, CurrentGameUserDto>().ReverseMap();
        CreateMap<PreviousGame, PreviousGameDto>().ReverseMap();
        CreateMap<PreviousGameUser, PreviousGameUserDto>().ReverseMap();
    }
}