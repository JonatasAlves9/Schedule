﻿using Microsoft.Extensions.Logging;
using ScheduleApplication.Data;
using ScheduleApplication.Services.Interface;
using ScheduleDomain.Entities;
using ScheduleDomain.Interface;

namespace ScheduleApplication.Services;

public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly IDoctorScheduleRepository _doctorScheduleRepository;
    private ILogger _logger;
    public DoctorScheduleService(IDoctorScheduleRepository doctorScheduleRepository, ILogger loger)
    {
        _doctorScheduleRepository = doctorScheduleRepository;
        _logger = loger;
    }
    public Result CreateSchedule(Guid doctorId, DateTime date, int startDate, int endDate)
    {
        try
        {
            DateTime start = new DateTime(1, 1, 1, startDate, 0, 0);
            DateTime end = new DateTime(1, 1, 1, endDate, 0, 0);

            if (start >= end)
            {
                return Result.FailResult("A hora de início deve ser anterior à hora de fim.");
            }

            List<DoctorSchedule> schedules = new List<DoctorSchedule>();

            for (DateTime time = start; time < end; time = time.AddHours(1))
            {
                DoctorSchedule model = new DoctorSchedule();

                model
                    .SetStartTime(time.TimeOfDay)
                    .SetFinishTime(time.AddHours(1).TimeOfDay)
                    .SetDate(date)
                    .SetDoctor(doctorId);
                
                schedules.Add(model);
            }

            List<DoctorSchedule> schedulesCreated = _doctorScheduleRepository.CreateDoctorSchedule(schedules, doctorId);
            
            if(schedulesCreated.Any())
                return Result.ObjectResult(schedulesCreated, "Sucesso ao cadastrar os horários");

            return Result.FailResult("Ocorreu um erro ao cadastrar os horários");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.FailResult(e.Message);
        }
    }
    
    public Result UpdateSchedule(Guid doctorScheduleId, Guid patientId, DateTime date, TimeSpan startTime, TimeSpan endTime)
    {
        try
        {
            DoctorSchedule? model = _doctorScheduleRepository.GetById(doctorScheduleId);
            
            if(model == null)
                return Result.FailResult("Horário não encontrado!");

            model
                .SetPatient(patientId)
                .SetDate(date)
                .SetFinishTime(endTime)
                .SetStartTime(startTime);
            
            return Result.SuccessResult("Horário atualizado com sucesso!");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.FailResult(e.Message);
        }
    }

    public Result DeleteSchedule(Guid doctorScheduleId)
    {
        try
        {
            DoctorSchedule? model = _doctorScheduleRepository.GetById(doctorScheduleId);

            if (model == null)
                return Result.FailResult("Horário não encontrado!");
                
            _doctorScheduleRepository.DeleteById(model.Id);
            
            return Result.SuccessResult("Horário cancelado com sucesso!");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.FailResult(e.Message);
        }
    }

    public Result CancelSchedule(Guid doctorScheduleId)
    {
        try
        {
            DoctorSchedule? model = _doctorScheduleRepository.GetById(doctorScheduleId);

            if (model == null)
                return Result.FailResult("Horário não encontrado!");
                
            _doctorScheduleRepository.CancelSchedule(model.Id);
            
            return Result.SuccessResult("Horário cancelado com sucesso!");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.FailResult(e.Message);
        }
    }

    public Result GetDoctorsAvaliableHours(Guid doctorId)
    {
        try
        {
            List<DoctorSchedule> list = _doctorScheduleRepository.GetListByDoctorIdAsync(doctorId).ToList();

            return Result.ObjectResult(list);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return Result.FailResult(e.Message);
        }
    }
}

