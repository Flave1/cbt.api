﻿using CBT.BLL.Constants;
using CBT.Contracts;
using CBT.Contracts.Authentication;
using CBT.Contracts.Candidates;
using CBT.Contracts.Category;
using CBT.Contracts.Common;
using CBT.Contracts.Question;
using CBT.DAL;
using CBT.DAL.Models.Candidate;
using CBT.DAL.Models.Question;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT.BLL.Services.Questions
{
    public class QuestionService : IQuestionService
    {
        private readonly DataContext _context;

        public QuestionService(DataContext context)
        {
            _context = context;
        }
        public async Task<APIResponse<CreateQuestion>> CreateQuestion(CreateQuestion request, Guid clientId, int userType)
        {
            var res = new APIResponse<CreateQuestion>();
            try
            {
                var examination = await _context.Examination.Where(m => m.ExaminationId == request.ExaminationId).FirstOrDefaultAsync();
                if (examination == null)
                {
                    res.IsSuccessful = false;
                    res.Message.FriendlyMessage = "ExaminationId doesn't exist";
                    return res;
                }

                string options = "";
                string answers = "";

                foreach(string item in request.Options)
                {
                    if(item.Contains("</option>"))
                    {
                        options = $"{options}{item}";
                    }
                    else
                    {
                        options = $"{options}{item}</option>";
                    }
                    
                }

                foreach(int item in request.Answers)
                {
                   answers = $"{answers}{item},";
                }

                var question = new Question
                {
                    QuestionText = request.QuestionText,
                    ExaminationId = request.ExaminationId,
                    Mark = request.Mark,
                    Options = options,
                    Answers = answers,
                    QuestionType = request.QuestionType,
                    ClientId = clientId,
                    UserType = userType
                };

                _context.Question.Add(question);
                await _context.SaveChangesAsync();
                res.Result = request;
                res.IsSuccessful = true;
                res.Message.FriendlyMessage = Messages.Created;
                return res;
            }
            catch (Exception ex)
            {
                res.IsSuccessful = false;
                res.Message.FriendlyMessage = Messages.FriendlyException;
                res.Message.TechnicalMessage = ex.ToString();
                return res;
            }
        }

        public async Task<APIResponse<IEnumerable<SelectQuestion>>> GetAllQuestions()
        {
            var res = new APIResponse<IEnumerable<SelectQuestion>>();
            try
            {
                var questions = await _context.Question
                    .OrderByDescending(s => s.CreatedOn)
                    .Where(d => d.Deleted != true).ToListAsync();


                var result = questions.Select(a => new SelectQuestion
                {
                    QuestionId = a.QuestionId,
                    QuestionText = a.QuestionText,
                    ExaminationId = a.ExaminationId,
                    Mark = a.Mark,
                    Options = a.Options.Split("</option>").SkipLast(1).ToArray(),
                    Answers = a.Answers.Split(",").SkipLast(1).ToArray(),
                    QuestionType = a.QuestionType,
                }).ToList();

                

                foreach(var item in result)
                {
                    string[] arr = new string[item.Answers.Count()];
                    int count = 0;
                    foreach (string answer in item.Answers)
                    {
                        //item.Answers = item.Answers.Append(item.Options[int.Parse(answer)]).ToArray();
                        arr[count] = item.Options[int.Parse(answer)];
                        //item.Answers = arr.Append(item.Options[int.Parse(answer)]).ToArray();
                        count++;
                    }
                    item.Answers = arr;
                    //Array.Clear(item.Answers, 0, count);
                    //item.Answers.SkipWhile(x => x.IsNullOrEmpty());
                }

                res.IsSuccessful = true;
                res.Result = result;
                res.Message.FriendlyMessage = Messages.GetSuccess;
                return res;
            }
            catch (Exception ex)
            {
                res.IsSuccessful = false;
                res.Message.FriendlyMessage = Messages.FriendlyException;
                res.Message.TechnicalMessage = ex.ToString();
                return res;
            }
        }

        public  async Task<APIResponse<SelectQuestion>> GetQuestion(Guid Id)
        {
            var res = new APIResponse<SelectQuestion>();
            try
            {
                var question = await _context.Question.Where(m => m.Deleted != true && m.QuestionId == Id).FirstOrDefaultAsync();
                if (question == null)
                {
                    res.IsSuccessful = false;
                    res.Message.FriendlyMessage = Messages.FriendlyNOTFOUND;
                    return res;
                }

                var result =  new SelectQuestion
                {
                    QuestionId = question.QuestionId,
                    QuestionText = question.QuestionText,
                    ExaminationId = question.ExaminationId,
                    Mark = question.Mark,
                    Options = question.Options.Split("</option>").SkipLast(1).ToArray(),
                    Answers = question.Answers.Split(",").SkipLast(1).ToArray(),
                    QuestionType = question.QuestionType,
                };


                string[] arr = new string[result.Answers.Count()];
                int count = 0;
                foreach (string answer in result.Answers)
                {
                    arr[count] = result.Options[int.Parse(answer)];
                    count++;
                }
                result.Answers = arr;

                res.IsSuccessful = true;
                res.Result = result;
                res.Message.FriendlyMessage = Messages.GetSuccess;
                return res;
            }
            catch (Exception ex)
            {
                res.IsSuccessful = false;
                res.Message.FriendlyMessage = Messages.FriendlyException;
                res.Message.TechnicalMessage = ex.ToString();
                return res;
            }
        }

        public async Task<APIResponse<UpdateQuestion>> UpdateQuestion(UpdateQuestion request)
        {
            var res = new APIResponse<UpdateQuestion>();
            try
            {
                var question = await _context.Question.Where(m => m.QuestionId == request.QuestionId).FirstOrDefaultAsync();
                if (question == null)
                {
                    res.IsSuccessful = false;
                    res.Message.FriendlyMessage = "QuestionId doesn't exist";
                    return res;
                }

                string options = "";
                string answers = "";

                foreach (string item in request.Options)
                {
                    if (item.Contains("</option>"))
                    {
                        options = $"{options}{item}";
                    }
                    else
                    {
                        options = $"{options}{item}</option>";
                    }

                }

                foreach (int item in request.Answers)
                {
                    answers = $"{answers}{item},";
                }

                question.QuestionText = request.QuestionText;
                question.ExaminationId = request.ExaminationId;
                question.Mark = request.Mark;
                question.Options = options;
                question.Answers = answers;
                question.QuestionType = request.QuestionType;

                await _context.SaveChangesAsync();
                res.Result = request;
                res.IsSuccessful = true;
                res.Message.FriendlyMessage = Messages.Updated;
                return res;
            }
            catch (Exception ex)
            {
                res.IsSuccessful = false;
                res.Message.FriendlyMessage = Messages.FriendlyException;
                res.Message.TechnicalMessage = ex.ToString();
                return res;
            }
        }
        public async Task<APIResponse<bool>> DeleteQuestion(SingleDelete request)
        {
            var res = new APIResponse<bool>();
            try
            {
                var question = await _context.Question.Where(d => d.Deleted != true && d.QuestionId == Guid.Parse(request.Item)).FirstOrDefaultAsync();
                if (question == null)
                {
                    res.Message.FriendlyMessage = "QuestionId does not exist";
                    res.IsSuccessful = true;
                    return res;
                }

                question.Deleted = true;
                await _context.SaveChangesAsync();

                res.IsSuccessful = true;
                res.Message.FriendlyMessage = Messages.DeletedSuccess;
                res.Result = true;
                return res;
            }
            catch (Exception ex)
            {
                res.IsSuccessful = false;
                res.Message.FriendlyMessage = Messages.FriendlyException;
                res.Message.TechnicalMessage = ex.ToString();
                return res;
            }
        }

    }
}
