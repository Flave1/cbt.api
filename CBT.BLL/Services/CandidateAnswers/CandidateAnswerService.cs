﻿using CBT.BLL.Constants;
using CBT.BLL.Filters;
using CBT.BLL.Services.Pagination;
using CBT.BLL.Services.Questions;
using CBT.BLL.Wrappers;
using CBT.Contracts;
using CBT.Contracts.CandidateAnswers;
using CBT.Contracts.Candidates;
using CBT.Contracts.Common;
using CBT.Contracts.Questions;
using CBT.DAL;
using CBT.DAL.Models.Candidate;
using CBT.DAL.Models.Questions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CBT.BLL.Services.CandidateAnswers
{
    public class CandidateAnswerService : ICandidateAnswerService
    {
        private readonly DataContext context;
        private readonly IHttpContextAccessor accessor;
        private readonly IPaginationService paginationService;

        public CandidateAnswerService(DataContext context, IHttpContextAccessor accessor, IPaginationService paginationService)
        {
            this.context = context;
            this.accessor = accessor;
            this.paginationService = paginationService;
        }
        public async Task<APIResponse<CreateCandidateAnswer>> SubmitCandidateAnswer(CreateCandidateAnswer request)
        {
            var res = new APIResponse<CreateCandidateAnswer>();
            try
            {
                var candidateAnswers = await context.CandidateAnswer.FirstOrDefaultAsync(a => a.QuestionId == Guid.Parse(request.QuestionId) && a.CandidateId == request.CandidateId);

                if(candidateAnswers == null)
                {
                    var newCandidateAnswer = new CandidateAnswer
                    {
                        QuestionId = Guid.Parse(request.QuestionId),
                        CandidateId = request.CandidateId,
                        Answers = String.Join(",", request.Answers)
                    };
                    context.CandidateAnswer.Add(newCandidateAnswer);
                }
                else
                {
                    candidateAnswers.Answers = String.Join(",", request.Answers);
                }

                await context.SaveChangesAsync();
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
        public async Task<APIResponse<bool>> SubmitAllCandidateAnswer(SubmitAllAnswers request)
        {
            var res = new APIResponse<bool>();
            try
            {
                foreach (var item in request.Answers)
                {
                    var candidateAnswers = await context.CandidateAnswer.FirstOrDefaultAsync(a => a.QuestionId == Guid.Parse(item.QuestionId) && a.CandidateId == request.CandidateId);

                    if(candidateAnswers == null)
                    {
                        var newCandidateAnswer = new CandidateAnswer
                        {
                            QuestionId = Guid.Parse(item.QuestionId),
                            CandidateId = request.CandidateId,
                            Answers = String.Join(",", item.Answers)
                        };
                        context.CandidateAnswer.Add(newCandidateAnswer);
                    }
                    else
                    {
                        candidateAnswers.Answers = String.Join(",", item.Answers);
                    }
                    
                }

                await context.SaveChangesAsync();
                res.Result = true;
                res.Message.FriendlyMessage = Messages.Created;
                res.IsSuccessful = true;
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
        public async Task<APIResponse<PagedResponse<List<SelectCandidateAnswer>>>> GetAllCandidateAnswers(PaginationFilter filter)
        {
            var res = new APIResponse<PagedResponse<List<SelectCandidateAnswer>>>();
            try
            {
                var clientId = Guid.Parse(accessor.HttpContext.Items["userId"].ToString());

                var query = context.CandidateAnswer
                    .Where(d => d.Deleted != true && d.ClientId == clientId)
                    .Include(q => q.Question)
                    .OrderByDescending(s => s.CreatedOn);

                var totalRecord = query.Count();
                var result = await paginationService.GetPagedResult(query, filter).Select(db => new SelectCandidateAnswer(db)).ToListAsync();
                res.Result = paginationService.CreatePagedReponse(result, filter, totalRecord);

                res.IsSuccessful = true;
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

        public async Task<APIResponse<SelectCandidateAnswer>> GetCandidateAnswer(Guid Id)
        {
            var res = new APIResponse<SelectCandidateAnswer>();
            try
            {
                var result = await context.CandidateAnswer
                    .Where(d => d.Deleted != true && d.AnswerId == Id)
                    .Include(q => q.Question)
                    .Select(db => new SelectCandidateAnswer(db))
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    res.IsSuccessful = false;
                    res.Message.FriendlyMessage = Messages.FriendlyNOTFOUND;
                    return res;
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
        public async Task<APIResponse<bool>> DeleteCandidateAnswer(SingleDelete request)
        {
            var res = new APIResponse<bool>();
            try
            {
                var answer = await context.CandidateAnswer.Where(d => d.Deleted != true && d.AnswerId == Guid.Parse(request.Item)).FirstOrDefaultAsync();
                if (answer == null)
                {
                    res.Message.FriendlyMessage = "AnswerId doesn't exist";
                    res.IsSuccessful = false;
                    return res;
                }

                answer.Deleted = true;
                await context.SaveChangesAsync();

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
