import { useState } from "react";
import z from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Trash2 } from "lucide-react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { formatLocalDate } from "../../utils/formatLocalDate";
import { userNameRule } from "../../utils/validationRules";
import DefaultProfileImage from "../../components/Profile/DefaultProfileImage";
import toast from "react-hot-toast";
import api from "../../services/api";
import "./MyProfilePage.scss";
import StatsGrid from "../../components/Profile/StatsGrid";

const userSchema = z.object({
  avatar: z.any().optional(),
  userName: userNameRule,
});

export default function MyProfilePage({ currentUser }) {
  const [userName, setUserName] = useState();
  const [isEditingName, setIsEditingName] = useState(false);
  const [avatarPreview, setAvatarPreview] = useState(currentUser?.avatarUrl);
  const [isRemoveAvatar, setIsRemoveAvatar] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(userSchema),
    defaultValues: {
      userName: currentUser?.userName,
    },
  });

  const updateMutation = useMutation({
    mutationFn: (formData) => api.put("users/me", formData),
    onSuccess: () => {
      queryClient.invalidateQueries(["my-profile"]);
      setIsEditingName(false);
      setHasChanges(false);
      toast.success("Профіль успішно оновлено");
    },
  });

  const enableNameEdit = () => {
    setIsEditingName(true);
  };

  const handleNameChange = (e) => {
    setUserName(e.target.value);
    setValue("userName", e.target.value, { shouldValidate: true });
    setHasChanges(true);
  };

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setAvatarPreview(URL.createObjectURL(file));
      setIsRemoveAvatar(false);
      setValue("avatar", file);
      setHasChanges(true);
    }
  };

  const handleRemoveAvatar = (e) => {
    e.stopPropagation();
    setAvatarPreview(null);
    setIsRemoveAvatar(true);
    setValue("avatar", null);
    setHasChanges(true);
  };

  const onSubmit = (data) => {
    // backend accepts [FromForm]
    const formData = new FormData();
    if (data.userName) {
      formData.append("UserName", data.userName);
    }
    if (data.avatar instanceof File) {
      formData.append("AvatarFile", data.avatar);
    }
    if (isRemoveAvatar){
      formData.append("RemoveAvatar", true );
    }
    updateMutation.mutate(formData);
  };

  return (
    <>
      <div className="profile-container">
        <form id="profileForm" onSubmit={handleSubmit(onSubmit)}>
          <div className="profile-header">
            <div className="avatar-section">
              <div
                className="avatar-wrapper"
                onClick={() =>
                  document.getElementById("avatarFileInput").click()
                }
              >
                <div className="profile-avatar">
                  {avatarPreview ? (
                    <img src={avatarPreview} id="avatarPreview" alt="Avatar" />
                  ) : (
                    <div className="avatar-placeholder">
                      <DefaultProfileImage />
                    </div>
                  )}
                </div>
                <div className="avatar-overlay avatar-overlay--desktop">
                  Змінити
                </div>
              </div>
              <input
                type="file"
                id="avatarFileInput"
                name="avatarFile"
                hidden
                accept="image/*"
                onChange={handleImageChange}
              />
              <button
                type="button"
                className="avatar-overlay avatar-overlay--mobile"
                onClick={() =>
                  document.getElementById("avatarFileInput").click()
                }
              >
                Змінити фото
              </button>

              {avatarPreview && (
                <button
                  type="button"
                  className="avatar-remove-btn"
                  onClick={handleRemoveAvatar}
                  title="Видалити зображення"
                >
                  <Trash2 size={16} />
                </button>
              )}
            </div>

            <table className="profile-info-table">
              <tbody className="profile-main-info">
                <tr>
                  <td className="label">Нікнейм:</td>
                  <td>
                    {!isEditingName ? (
                      <span
                        id="userNameText"
                        className="info-value-clickable"
                        onClick={enableNameEdit}
                        title="Змінити"
                      >
                        {currentUser?.userName}
                      </span>
                    ) : (
                      <div className="inline-input-container">
                        <input
                          type="text"
                          id="userNameInput"
                          className={
                            errors.userName
                              ? "inline-input error-input"
                              : "inline-input"
                          }
                          value={userName}
                          {...register("userName", {
                            onChange: handleNameChange,
                          })}
                          autoFocus
                        />
                        {errors.userName && (
                          <span
                            className="error-message"
                            style={{
                              color: "#ff8080",
                              display: "block",
                              fontSize: "0.8rem",
                              marginTop: "5px",
                            }}
                          >
                            {errors.userName.message}
                          </span>
                        )}
                      </div>
                    )}
                  </td>
                </tr>
                <tr>
                  <td className="label">Email:</td>
                  <td>
                    <span className="info-value info-value--email">
                      {currentUser?.email}
                    </span>
                  </td>
                </tr>
                <tr>
                  <td className="label">Реєстрація:</td>
                  <td>
                    <span className="info-value info-value--date">
                      {formatLocalDate(currentUser?.createdAt, {
                        withTime: false,
                      })}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          {hasChanges && (
            <button
              type="submit"
              id="saveChangesBtn"
              className="profile-save-btn"
              disabled={isSubmitting}
            >
              {updateMutation.isPending ? "Збереження..." : "Зберегти зміни"}
            </button>
          )}
        </form>

        <StatsGrid user={currentUser} />
      </div>
    </>
  );
}
